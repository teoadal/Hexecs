using System.Collections.Concurrent;
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
    public event Action<uint>? Created;

    /// <summary>
    /// Событие, вызываемое в начале удаления актёра.
    /// </summary>
    public event Action<uint>? Destroying;

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

    internal ActorContext(bool isDefault,
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

        _buckets = new int[capacity];
        _entries = new Entry[capacity];
        _freeCount = 0;
        _freeList = 0;
        _length = 0;

        _builders = [];

        _commands = [];
        _queries = [];
        _notifications = [];
        _messages = [];
        _messageGroups = FrozenDictionary<string, MessageQueueGroup>.Empty;

        _componentPools = new IActorComponentPool?[32];
        _componentPoolLock = new Lock();
        _componentConfigurations = componentConfigurations;

        _filters = new Dictionary<Type, IActorFilter>(8, ReferenceComparer<Type>.Instance);
        _filtersWithConstraint = new List<IActorFilter>(8);

        _relationPools = new IActorRelationPool?[32];
        _relationPoolLock = new Lock();

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
    public bool ActorAlive(uint actorId)
    {
        ref var entry = ref GetEntry(actorId);
        return !Unsafe.IsNullRef(ref entry) && entry.Key == actorId;
    }

    /// <summary>
    /// Очищает контекст актёров, удаляя всех актёров и их компоненты.
    /// Вызывает событие Cleared по завершении.
    /// </summary>
    public void Clear()
    {
        Cleared?.Invoke();

        foreach (var componentPool in _componentPools)
        {
            componentPool?.Clear();
        }

        foreach (var relationPool in _relationPools)
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
    public Actor Clone(uint actorId, bool withParent = true)
    {
        var cloneId = GetNextActorId();
        ref var cloneEntry = ref AddEntry(cloneId);

        ref var entry = ref GetEntryExact(actorId);
        foreach (var componentId in entry.Components)
        {
            var componentPool = _componentPools[componentId]!;
            componentPool.Clone(actorId, cloneId);

            cloneEntry.Components.Add(componentId);
        }

        // ReSharper disable once InvertIf
        if (withParent && TryGetParent(actorId, out var parent))
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
        if (expectedId == Actor.EmptyId) ActorError.WrongId();
        var actorId = expectedId ?? GetNextActorId();

        AddEntry(actorId);
        return new Actor(this, actorId);
    }

    /// <summary>
    /// Уничтожает актёра с указанным идентификатором.
    /// </summary>
    /// <param name="actorId">Идентификатор уничтожаемого актёра</param>
    /// <returns>Возвращает true, если актёр был успешно уничтожен, иначе false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool DestroyActor(uint actorId)
    {
        if (RemoveEntry(actorId))
        {
            _freeIds.Push(actorId);
            return true;
        }

        return false;
    }

    public void Dispose()
    {
        Clear();

        foreach (var filter in _filters.Values)
        {
            filter.Dispose();
        }

        _filters.Clear();

        foreach (var filter in _filtersWithConstraint)
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
    public Actor GetActor(uint actorId)
    {
        if (!ActorAlive(actorId)) ActorError.NotFound(actorId);
        return new Actor(this, actorId);
    }

    /// <summary>
    /// Получает первого актёра с компонентом указанного типа.
    /// </summary>
    /// <typeparam name="T1">Тип компонента</typeparam>
    /// <returns>Первый найденный актёр с компонентом указанного типа</returns>
    /// <exception cref="Exception">Возникает, если актёр с компонентом указанного типа не найден</exception>
    public Actor<T1> GetActor<T1>()
        where T1 : struct, IActorComponent
    {
        var pool = GetComponentPool<T1>();
        if (pool is { Length: > 0 }) return pool.First();

        ActorError.NotFound<T1>();
        return Actor<T1>.Empty;
    }

    /// <summary>
    /// Получает первого актёра с компонентом указанного типа, удовлетворяющего предикату.
    /// </summary>
    /// <typeparam name="T1">Тип компонента</typeparam>
    /// <param name="predicate">Предикат для фильтрации актёров</param>
    /// <returns>Первый найденный актёр, удовлетворяющий предикату</returns>
    /// <exception cref="Exception">Возникает, если подходящий актёр не найден</exception>
    public Actor<T1> GetActor<T1>(ActorPredicate<T1> predicate)
        where T1 : struct, IActorComponent
    {
        var pool = GetComponentPool<T1>();
        if (pool != null)
        {
            var exists = pool.First(predicate);
            if (exists) return new Actor<T1>(this, exists.Id);
        }

        ActorError.ApplicableNotFound<T1>();
        return Actor<T1>.Empty;
    }

    /// <summary>
    /// Получает актёра с указанным идентификатором и компонентом указанного типа.
    /// </summary>
    /// <typeparam name="T1">Тип компонента</typeparam>
    /// <param name="actorId">Идентификатор актёра</param>
    /// <returns>Актёр с указанным идентификатором и компонентом</returns>
    /// <exception cref="Exception">Возникает, если актёр не найден или не содержит указанный компонент</exception>
    public Actor<T1> GetActor<T1>(uint actorId)
        where T1 : struct, IActorComponent
    {
        var pool = GetComponentPool<T1>();
        if (pool == null || !pool.Has(actorId)) ActorError.ComponentNotFound<T1>(actorId);

        return new Actor<T1>(this, actorId);
    }

    /// <summary>
    /// Получает ссылку на первого актёра с компонентом указанного типа.
    /// </summary>
    /// <typeparam name="T1">Тип компонента</typeparam>
    /// <returns>Ссылка на первого найденного актёра с компонентом указанного типа</returns>
    /// <exception cref="Exception">Возникает, если актёр с компонентом указанного типа не найден</exception>
    public ActorRef<T1> GetActorRef<T1>()
        where T1 : struct, IActorComponent
    {
        var pool = GetComponentPool<T1>();
        if (pool is { Length: > 0 }) return pool.First();

        ActorError.NotFound<T1>();
        return ActorRef<T1>.Empty;
    }

    /// <summary>
    /// Получает ссылку на первого актёра с компонентом указанного типа, удовлетворяющего предикату.
    /// </summary>
    /// <typeparam name="T1">Тип компонента</typeparam>
    /// <param name="predicate">Предикат для фильтрации актёров</param>
    /// <returns>Ссылка на первого найденного актёра, удовлетворяющего предикату</returns>
    /// <exception cref="Exception">Возникает, если подходящий актёр не найден</exception>
    public ActorRef<T1> GetActorRef<T1>(ActorPredicate<T1> predicate)
        where T1 : struct, IActorComponent
    {
        var pool = GetComponentPool<T1>();
        if (pool != null)
        {
            var exists = pool.First(predicate);
            if (exists) return exists;
        }

        ActorError.ApplicableNotFound<T1>();
        return ActorRef<T1>.Empty;
    }

    /// <summary>
    /// Получает ссылку на актёра с указанным идентификатором и компонентом указанного типа.
    /// </summary>
    /// <typeparam name="T1">Тип компонента</typeparam>
    /// <param name="actorId">Идентификатор актёра</param>
    /// <returns>Ссылка на актёра с указанным идентификатором и компонентом</returns>
    /// <exception cref="Exception">Возникает, если актёр не найден или не содержит указанный компонент</exception>
    public ActorRef<T1> GetActorRef<T1>(uint actorId)
        where T1 : struct, IActorComponent
    {
        ref var component = ref TryGetComponentRef<T1>(actorId);
        if (Unsafe.IsNullRef(ref component)) ActorError.ComponentNotFound<T1>(actorId);

        return new ActorRef<T1>(this, actorId, ref component);
    }

    /// <summary>
    /// Получает текстовое описание актёра.
    /// </summary>
    /// <param name="actorId">Идентификатор актёра</param>
    /// <param name="maxComponentDescription">Максимальное количество отображаемых компонентов</param>
    /// <returns>Текстовое описание актёра</returns>
    [SkipLocalsInit]
    public string GetDescription(uint actorId, int maxComponentDescription = 5)
    {
        var builder = new ValueStringBuilder(stackalloc char[512]);
        GetDescription(actorId, ref builder, maxComponentDescription);
        return builder.Flush();
    }


    public void GetDescription(uint actorId, ref ValueStringBuilder builder, int maxComponentDescription = 5)
    {
        ref var entry = ref GetEntry(actorId);
        if (Unsafe.IsNullRef(ref entry))
        {
            builder.Append('\'');
            builder.Append(StringUtils.EmptyValue);
            builder.Append('\'');
        }

        builder.Append("Id = ");
        builder.Append(actorId);

        ref var components = ref entry.Components;
        var componentsLength = components.Length;
        if (componentsLength == 0) return;

        builder.Append(" (");

        var pool = ArrayPool<string>.Shared;
        var buffer = pool.Rent(componentsLength);
        var index = 0;
        var printMore = false;

        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var componentId in components)
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
                TypeOf.GetTypeName(ActorComponentType.GetType(componentId))
            );
        }

        Array.Sort(buffer, 0, componentsLength);

        var first = true;
        foreach (var componentName in buffer.AsSpan(0, index))
        {
            if (first == false) builder.Append(", ");
            else first = false;

            builder.Append(componentName);
        }

        if (printMore) builder.Append(", ...");
        builder.Append(')');
    }

    /// <summary>
    /// Получает единственный актёр с компонентом указанного типа.
    /// </summary>
    /// <typeparam name="T1">Тип компонента</typeparam>
    /// <returns>Единственный актёр с компонентом указанного типа</returns>
    /// <exception cref="Exception">Возникает, если актёр не найден или найдено более одного актёра</exception>
    public Actor<T1> Single<T1>()
        where T1 : struct, IActorComponent
    {
        var componentId = ActorComponentType<T1>.Id;
        if (_singles.TryGetValue(componentId, out var exists))
        {
            return new Actor<T1>(this, exists);
        }

        var pool = GetComponentPool<T1>();
        if (pool == null) ActorError.SingleNotFound<T1>();
        if (pool.Length > 1) ActorError.NotSingle<T1>();

        var single = pool.First();
        _singles.Add(componentId, single.Id);

        return single;
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
        var componentId = ActorComponentType<T1>.Id;
        if (_singles.ContainsKey(componentId))
        {
            var componentPool = (ActorComponentPool<T1>)_componentPools[componentId]!;
            return componentPool.First();
        }

        var pool = GetComponentPool<T1>();
        if (pool == null) ActorError.SingleNotFound<T1>();
        if (pool.Length > 1) ActorError.NotSingle<T1>();

        var single = pool.First();
        _singles.Add(componentId, single.Id);

        return single;
    }

    /// <summary>
    /// Пытается получить актёра с указанным идентификатором и компонентом указанного типа.
    /// </summary>
    /// <typeparam name="T1">Тип компонента</typeparam>
    /// <param name="actorId">Идентификатор актёра</param>
    /// <param name="actor">Переменная для сохранения найденного актёра</param>
    /// <returns>Возвращает true, если актёр найден и содержит указанный компонент, иначе false</returns>
    public bool TryGetActor<T1>(uint actorId, out Actor<T1> actor)
        where T1 : struct, IActorComponent
    {
        var pool = GetComponentPool<T1>();
        if (pool == null || !pool.Has(actorId))
        {
            actor = Actor<T1>.Empty;
            return false;
        }

        actor = new Actor<T1>(this, actorId);
        return true;
    }

    /// <summary>
    /// Пытается получить ссылку на актёра с указанным идентификатором и компонентом указанного типа.
    /// </summary>
    /// <typeparam name="T1">Тип компонента</typeparam>
    /// <param name="actorId">Идентификатор актёра</param>
    /// <param name="actor">Переменная для сохранения ссылки на найденного актёра</param>
    /// <returns>Возвращает true, если актёр найден и содержит указанный компонент, иначе false</returns>
    public bool TryGetActorRef<T1>(uint actorId, out ActorRef<T1> actor)
        where T1 : struct, IActorComponent
    {
        ref var component = ref TryGetComponentRef<T1>(actorId);
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
        if (_freeIds.TryPop(out var reusedId))
        {
            return reusedId;
        }

        var actorId = Interlocked.Increment(ref _nextActorId);
        while (ActorAlive(actorId))
        {
            actorId = Interlocked.Increment(ref _nextActorId);
        }

        return actorId;
    }
}