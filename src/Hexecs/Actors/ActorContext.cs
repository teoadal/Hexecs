using System.Collections.Frozen;

using Hexecs.Actors.Components;
using Hexecs.Actors.Delegates;
using Hexecs.Actors.Relations;
using Hexecs.Collections;
using Hexecs.Dependencies;
using Hexecs.Pipelines.Messages;
using Hexecs.Worlds;

namespace Hexecs.Actors;

/// <summary>
/// Контекст актёров представляет собой коллекцию сущностей и их компонентов в игровом мире.
/// Класс управляет жизненным циклом актёров, их компонентами и взаимосвязями.
/// </summary>
[DebuggerDisplay("Length = {Length}")]
public sealed partial class ActorContext : IEnumerable<Actor>, IDisposable
{
    /// <summary>
    /// Событие, вызываемое при очистке всего контекста актёров.
    /// </summary>
    public event Action? Cleared;

    /// <summary>
    /// Событие, вызываемое при завершении создания актёра.
    /// </summary>
    public event Action<ActorId>? Created;

    /// <summary>
    /// Событие, вызываемое в начале удаления актёра.
    /// </summary>
    public event Action<ActorId>? Destroying;

    /// <summary>
    /// Уникальный идентификатор контекста актёров.
    /// </summary>
    public readonly int Id;

    /// <summary>
    /// Флаг, указывающий является ли данный контекст контекстом по умолчанию.
    /// </summary>
    public readonly bool IsDefault;

    /// <summary>
    /// Мир, к которому принадлежит данный контекст актёров.
    /// </summary>
    public readonly World World;

    private readonly ThreadLocalStack<uint> _freeIds;
    private uint _nextActorId;
    private readonly Dictionary<ushort, uint> _singles;

    internal ActorContext(
        bool isDefault,
        int id,
        DependencyProvider dependencyProvider,
        World world,
        int capacity,
        IActorComponentConfiguration?[] componentConfigurations)
    {
        Id = id;
        IsDefault = isDefault;
        World = world;

        capacity = HashHelper.GetPrime(capacity);

        _sparsePages = new uint[16][];
        _dense = new uint[capacity];
        _values = new Entry[capacity];

        _builders = [];

        _commands = [];
        _queries = [];
        _notifications = [];
        _messages = [];
        _messageGroups = FrozenDictionary<string, MessageQueueGroup>.Empty;

        _componentPools = new IActorComponentPool?[32];
        _componentConfigurations = componentConfigurations;

        _filters = new Dictionary<Type, IActorFilter>(8, ReferenceComparer<Type>.Instance);
        _filtersWithConstraint = new List<IActorFilter>(8);

        _relationPools = new IActorRelationPool?[32];

        _freeIds = new ThreadLocalStack<uint>(capacity);
        _nextActorId = 0;
        _singles = new Dictionary<ushort, uint>();

        _drawSystems = [];
        _updateSystems = [];

        _dependencyProvider = dependencyProvider;
        _dependencyProvider.Add(DependencyKey.First(typeof(ActorContext)), this);
    }

    /// <summary>
    /// Проверяет, существует ли актёр с указанным идентификатором.
    /// </summary>
    /// <param name="actorId">Идентификатор актёра для проверки</param>
    /// <returns>Возвращает true, если актёр существует, иначе false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ActorAlive(ActorId actorId)
    {
        return !Unsafe.IsNullRef(ref GetEntryRef(actorId.Value));
    }

    /// <summary>
    /// Очищает контекст актёров, удаляя всех актёров и их компоненты.
    /// Вызывает событие Cleared по завершении.
    /// </summary>
    public void Clear()
    {
        Cleared?.Invoke();

        foreach (IActorComponentPool? componentPool in _componentPools)
        {
            componentPool?.Clear();
        }

        foreach (IActorRelationPool? relationPool in _relationPools)
        {
            relationPool?.Clear();
        }

        ClearEntries();

        _singles.Clear();
        _freeIds.Clear();
    }

    /// <summary>
    /// Создаёт клон существующего актёра со всеми его компонентами.
    /// </summary>
    /// <param name="actorId">Идентификатор клонируемого актёра</param>
    /// <param name="withParent">Флаг, указывающий нужно ли сохранять родительскую связь</param>
    /// <returns>Новый актёр, являющийся клоном исходного</returns>
    /// <exception cref="Exception">Возникает, если актёр с указанным идентификатором не найден</exception>
    public Actor Clone(ActorId actorId, bool withParent = true)
    {
        uint cloneIdRaw = GetNextActorId();
        ref Entry cloneEntry = ref AddEntry(cloneIdRaw);

        var cloneId = new ActorId(cloneIdRaw);
        ref Entry entry = ref GetEntryRefExact(actorId.Value);

        foreach (ushort componentId in entry)
        {
            IActorComponentPool componentPool = _componentPools[componentId]!;
            componentPool.Clone(actorId, cloneId);

            cloneEntry.Add(componentId);
        }

        // ReSharper disable once InvertIf
        if (withParent && TryGetParent(actorId, out Actor parent))
        {
            AddChild(parent.Id, cloneId);
        }

        return new Actor(this, cloneId);
    }

    /// <summary>
    /// Создаёт нового актёра с указанным или автоматически сгенерированным идентификатором.
    /// </summary>
    /// <param name="expectedId">Ожидаемый идентификатор создаваемого актёра (если null, будет сгенерирован автоматически)</param>
    /// <returns>Новый созданный актёр</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Actor CreateActor(uint? expectedId = null)
    {
        if (expectedId == ActorId.EmptyId)
        {
            ActorError.InvalidId();
        }

        uint actorId = expectedId ?? GetNextActorId();

        AddEntry(actorId);

        return new Actor(this, new ActorId(actorId));
    }

    /// <summary>
    /// Уничтожает актёра с указанным идентификатором.
    /// </summary>
    /// <param name="actorId">Идентификатор уничтожаемого актёра</param>
    /// <returns>Возвращает true, если актёр был успешно уничтожен, иначе false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool DestroyActor(ActorId actorId)
    {
        if (RemoveEntry(actorId.Value))
        {
            _freeIds.Push(actorId.Value);

            return true;
        }

        return false;
    }

    public void Dispose()
    {
        Clear();

        foreach (IActorFilter filter in _filters.Values)
        {
            filter.Dispose();
        }

        _filters.Clear();

        foreach (IActorFilter filter in _filtersWithConstraint)
        {
            filter.Dispose();
        }

        _filtersWithConstraint.Clear();

        _freeIds.Dispose();

        _dependencyProvider.Dispose();
    }

    /// <summary>
    /// Получает актёра по его идентификатору.
    /// </summary>
    /// <param name="actorId">Идентификатор актёра</param>
    /// <returns>Актёр с указанным идентификатором</returns>
    /// <exception cref="Exception">Возникает, если актёр с указанным идентификатором не найден</exception>
    public Actor GetActor(ActorId actorId)
    {
        if (!ActorAlive(actorId))
        {
            ActorError.NotFound(actorId);
        }

        return new Actor(this, actorId);
    }

    /// <summary>
    /// Получает первого актёра с компонентом указанного типа.
    /// </summary>
    /// <typeparam name="T1">Тип компонента</typeparam>
    /// <returns>Первый найденный актёр с компонентом указанного типа</returns>
    /// <exception cref="Exception">Возникает, если актёр с компонентом указанного типа не найден</exception>
    public ActorRef<T1> GetActorRef<T1>()
        where T1 : struct, IActorComponent
    {
        ActorComponentPool<T1>? pool = GetComponentPool<T1>();

        if (pool != null)
        {
            ActorRef<T1> first = pool.First();

            if (!first.IsEmpty)
            {
                return first;
            }
        }

        ActorError.NotFound<T1>();

        return ActorRef<T1>.Empty;
    }

    /// <summary>
    /// Получает ссылку на актёра с указанным идентификатором и компонентом указанного типа.
    /// </summary>
    /// <typeparam name="T1">Тип компонента</typeparam>
    /// <param name="actorId">Идентификатор актёра</param>
    /// <returns>Ссылка на актёра с указанным идентификатором и компонентом</returns>
    /// <exception cref="Exception">Возникает, если актёр не найден или не содержит указанный компонент</exception>
    public ActorRef<T1> GetActorRef<T1>(ActorId actorId)
        where T1 : struct, IActorComponent
    {
        ref T1 component = ref TryGetComponentRef<T1>(actorId);

        if (Unsafe.IsNullRef(ref component))
        {
            ActorError.ComponentNotFound<T1>(actorId);
        }

        return new ActorRef<T1>(this, actorId, ref component);
    }

    /// <summary>
    /// Получает первого актёра с компонентом указанного типа, удовлетворяющего предикату.
    /// </summary>
    /// <typeparam name="T1">Тип компонента</typeparam>
    /// <param name="predicate">Предикат для фильтрации актёров</param>
    /// <returns>Первый найденный актёр, удовлетворяющий предикату</returns>
    /// <exception cref="Exception">Возникает, если подходящий актёр не найден</exception>
    public ActorRef<T1> GetActorRef<T1>(ActorPredicate<T1> predicate)
        where T1 : struct, IActorComponent
    {
        ActorComponentPool<T1>? pool = GetComponentPool<T1>();

        if (pool != null)
        {
            ActorRef<T1> exists = pool.First(predicate);

            if (!exists.IsEmpty)
            {
                return exists;
            }
        }

        ActorError.ApplicableNotFound<T1>();

        return ActorRef<T1>.Empty;
    }

    /// <summary>
    /// Получает текстовое описание актёра.
    /// </summary>
    /// <param name="actorId">Идентификатор актёра</param>
    /// <param name="maxComponentDescription">Максимальное количество отображаемых компонентов</param>
    /// <returns>Текстовое описание актёра</returns>
    [SkipLocalsInit]
    public string GetDescription(ActorId actorId, int maxComponentDescription = 5)
    {
        var builder = new ValueStringBuilder(stackalloc char[512]);
        GetDescription(actorId, ref builder, maxComponentDescription);

        return builder.Flush();
    }

    public void GetDescription(ActorId actorId, ref ValueStringBuilder builder, int maxComponentDescription = 5)
    {
        ref Entry entry = ref GetEntryRef(actorId.Value);

        if (Unsafe.IsNullRef(ref entry))
        {
            builder.Append('\'');
            builder.Append(StringUtils.EmptyValue);
            builder.Append('\'');

            return;
        }

        builder.Append("Id = ");
        builder.Append(actorId.Value);

        int componentsLength = entry.Length;

        if (componentsLength == 0)
        {
            return;
        }

        builder.Append(" (");

        ArrayPool<string> pool = ArrayPool<string>.Shared;
        string[] buffer = pool.Rent(componentsLength);
        var index = 0;
        var printMore = false;

        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (ushort componentId in entry)
        {
            if (maxComponentDescription == index)
            {
                printMore = true;

                break;
            }

            ArrayUtils.Insert(
                ref buffer,
                pool,
                index++,
                TypeOf.GetTypeName(ActorComponentType.GetType(componentId)));
        }

        Array.Sort(buffer, 0, index);

        var first = true;

        foreach (string componentName in buffer.AsSpan(0, index))
        {
            if (first == false)
            {
                builder.Append(", ");
            }
            else
            {
                first = false;
            }

            builder.Append(componentName);
        }

        if (printMore)
        {
            builder.Append(", ...");
        }

        builder.Append(')');

        pool.Return(buffer);
    }

    /// <summary>
    /// Получает ссылку на единственного актёра с компонентом указанного типа.
    /// </summary>
    /// <typeparam name="T1">Тип компонента</typeparam>
    /// <returns>Ссылка на единственного актёра с компонентом указанного типа</returns>
    /// <exception cref="Exception">Возникает, если актёр не найден или найдено более одного актёра</exception>
    public ActorRef<T1> SingleRef<T1>()
        where T1 : struct, IActorComponent
    {
        ushort componentId = ActorComponentType<T1>.Id;

        if (_singles.ContainsKey(componentId))
        {
            var componentPool = (ActorComponentPool<T1>)_componentPools[componentId]!;

            return componentPool.First();
        }

        ActorComponentPool<T1>? pool = GetComponentPool<T1>();

        if (pool == null)
        {
            ActorError.SingleNotFound<T1>();
        }

        if (pool.Length > 1)
        {
            ActorError.NotSingle<T1>();
        }

        ActorRef<T1> single = pool.First();
        _singles.Add(componentId, single.Id.Value);

        return single;
    }

    /// <summary>
    /// Пытается получить ссылку на актёра с указанным идентификатором и компонентом указанного типа.
    /// </summary>
    /// <typeparam name="T1">Тип компонента</typeparam>
    /// <param name="actorId">Идентификатор актёра</param>
    /// <param name="actor">Переменная для сохранения ссылки на найденного актёра</param>
    /// <returns>Возвращает true, если актёр найден и содержит указанный компонент, иначе false</returns>
    public bool TryGetActorRef<T1>(ActorId actorId, out ActorRef<T1> actor)
        where T1 : struct, IActorComponent
    {
        ref T1 component = ref TryGetComponentRef<T1>(actorId);

        if (Unsafe.IsNullRef(ref component))
        {
            actor = ActorRef<T1>.Empty;

            return false;
        }

        actor = new ActorRef<T1>(this, actorId, ref component);

        return true;
    }

    /// <summary>
    /// Получает следующий доступный идентификатор актёра.
    /// </summary>
    /// <returns>Следующий доступный идентификатор актёра</returns>
    private uint GetNextActorId()
    {
        if (_freeIds.TryPop(out uint reusedId))
        {
            return reusedId;
        }

        uint actorId = Interlocked.Increment(ref _nextActorId);

        while (!Unsafe.IsNullRef(ref GetEntryRef(actorId))) // is alive
        {
            actorId = Interlocked.Increment(ref _nextActorId);
        }

        return actorId;
    }
}
