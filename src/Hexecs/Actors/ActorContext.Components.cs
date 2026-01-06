using Hexecs.Actors.Components;
using Hexecs.Actors.Delegates;

namespace Hexecs.Actors;

public sealed partial class ActorContext
{
    private IActorComponentPool?[] _componentPools;
#if NET9_0_OR_GREATER
    private readonly Lock _componentPoolLock = new();
#else
    private readonly object _componentPoolLock = new();
#endif
    private readonly IActorComponentConfiguration?[] _componentConfigurations;

    /// <summary>
    /// Добавляет компонент к указанному актёру.
    /// </summary>
    /// <typeparam name="T">Тип компонента. Должен быть структурой и реализовывать <see cref="IActorComponent"/>.</typeparam>
    /// <param name="actorId">Идентификатор актёра.</param>
    /// <param name="component">Компонент для добавления.</param>
    /// <returns>Ссылка на добавленный компонент в пуле.</returns>
    public ref T AddComponent<T>(uint actorId, in T component)
        where T : struct, IActorComponent
    {
        var pool = GetOrCreateComponentPool<T>();
        ref var result = ref pool.Add(actorId, in component);

        ref var entry = ref GetEntryExact(actorId);
        entry.Add(ActorComponentType<T>.Id);

        return ref result;
    }

    /// <summary>
    /// Клонирует компонент от одного актёра другому.
    /// </summary>
    /// <typeparam name="T">Тип компонента. Должен быть структурой и реализовывать <see cref="IActorComponent"/>.</typeparam>
    /// <param name="ownerId">Идентификатор актёра-владельца компонента.</param>
    /// <param name="cloneId">Идентификатор актёра, которому клонируется компонент.</param>
    /// <returns>Ссылка на клонированный компонент в пуле.</returns>
    /// <exception cref="Exception">Выбрасывается, если компонент типа <typeparamref name="T"/> не найден у актёра-владельца.</exception>
    /// <exception cref="Exception">Выбрасывается, если компонент типа <typeparamref name="T"/> уже существует у того актёра, которому клонируется компонент.</exception>
    public ref T CloneComponent<T>(uint ownerId, uint cloneId) where T : struct, IActorComponent
    {
        var pool = GetComponentPool<T>();
        if (pool == null) ActorError.ComponentNotFound<T>(ownerId);

        ref var clone = ref GetEntryExact(cloneId);
        if (clone.TryAdd(ActorComponentType<T>.Id))
        {
            return ref pool.Clone(ownerId, cloneId);
        }

        ActorError.ComponentExists<T>(cloneId);
        return ref Unsafe.NullRef<T>();
    }

    /// <summary>
    /// Возвращает перечислитель компонентов для указанного актёра.
    /// </summary>
    /// <param name="actorId">Идентификатор актёра.</param>
    /// <remarks>Не нужно использовать этот метод в "горячих" местах кода</remarks>
    /// <returns>Перечислитель компонентов. Возвращает <see cref="ComponentEnumerator.Empty"/>, если актёр не найден.</returns>
    public ComponentEnumerator Components(uint actorId)
    {
        ref var entry = ref GetEntry(actorId);
        return Unsafe.IsNullRef(ref entry) 
            ? ComponentEnumerator.Empty 
            : new ComponentEnumerator(actorId, _componentPools, entry.ToArray());
    }

    /// <summary>
    /// Получает компонент указанного типа для заданного актёра.
    /// </summary>
    /// <typeparam name="T">Тип компонента. Должен быть структурой и реализовывать <see cref="IActorComponent"/>.</typeparam>
    /// <param name="actorId">Идентификатор актёра.</param>
    /// <returns>Ссылка на компонент в пуле.</returns>
    /// <exception cref="Exception">Выбрасывается, если актёр не имеет компонента типа <typeparamref name="T"/>.</exception>
    public ref T GetComponent<T>(uint actorId) where T : struct, IActorComponent
    {
        var pool = GetComponentPool<T>();
        if (pool == null) ActorError.ComponentNotFound<T>(actorId);
        return ref pool.Get(actorId);
    }

    /// <summary>
    /// Получает компонент указанного типа для заданного актёра или добавляет его, используя фабричный метод, если компонент отсутствует.
    /// </summary>
    /// <typeparam name="T">Тип компонента. Должен быть структурой и реализовывать <see cref="IActorComponent"/>.</typeparam>
    /// <param name="actorId">Идентификатор актёра.</param>
    /// <param name="factory">Фабричный метод для создания компонента, если он отсутствует.</param>
    /// <returns>Ссылка на существующий или вновь созданный компонент.</returns>
    public ref T GetOrAddComponent<T>(uint actorId, Func<uint, T> factory)
        where T : struct, IActorComponent
    {
        var pool = GetOrCreateComponentPool<T>();
        ref var component = ref pool.GetOrCreate(actorId, out var added, factory);

        // ReSharper disable once InvertIf
        if (added)
        {
            ref var entry = ref GetEntryExact(actorId);
            entry.Add(ActorComponentType<T>.Id);
        }

        return ref component;
    }

    /// <summary>
    /// Проверяет, имеет ли указанный актёр компонент заданного типа.
    /// </summary>
    /// <typeparam name="T">Тип компонента. Должен быть структурой и реализовывать <see cref="IActorComponent"/>.</typeparam>
    /// <param name="actorId">Идентификатор актёра.</param>
    /// <returns><c>true</c>, если актёр имеет компонент; в противном случае <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasComponent<T>(uint actorId) where T : struct, IActorComponent
    {
        var pool = GetComponentPool<T>();
        return pool != null && pool.Has(actorId);
    }

    /// <summary>
    /// Регистрирует обработчик, который будет вызван после добавления компонента типа <typeparamref name="T"/> к любому актёру.
    /// </summary>
    /// <typeparam name="T">Тип компонента. Должен быть структурой и реализовывать <see cref="IActorComponent"/>.</typeparam>
    /// <param name="handler">Обработчик, принимающий идентификатор актёра.</param>
    public void OnComponentAdded<T>(Action<uint> handler)
        where T : struct, IActorComponent
    {
        var pool = GetOrCreateComponentPool<T>();
        pool.Added += handler;
    }

    /// <summary>
    /// Регистрирует обработчик, который будет вызван после добавления компонента типа <typeparamref name="T"/> к любому актёру.
    /// </summary>
    /// <typeparam name="T">Тип компонента. Должен быть структурой и реализовывать <see cref="IActorComponent"/>.</typeparam>
    /// <param name="handler">Обработчик, принимающий идентификатор актёра и ссылку на добавленный компонент.</param>
    public void OnComponentAdded<T>(ActorComponentAdded<T> handler)
        where T : struct, IActorComponent
    {
        var pool = GetOrCreateComponentPool<T>();
        pool.ComponentAdded += handler;
    }

    /// <summary>
    /// Регистрирует обработчик, который будет вызван перед удалением компонента типа <typeparamref name="T"/> у любого актёра.
    /// </summary>
    /// <typeparam name="T">Тип компонента. Должен быть структурой и реализовывать <see cref="IActorComponent"/>.</typeparam>
    /// <param name="handler">Обработчик, принимающий идентификатор актёра.</param>
    public void OnComponentRemoving<T>(Action<uint> handler)
        where T : struct, IActorComponent
    {
        var pool = GetOrCreateComponentPool<T>();
        pool.Removing += handler;
    }

    /// <summary>
    /// Регистрирует обработчик, который будет вызван перед удалением компонента типа <typeparamref name="T"/> у любого актёра.
    /// </summary>
    /// <typeparam name="T">Тип компонента. Должен быть структурой и реализовывать <see cref="IActorComponent"/>.</typeparam>
    /// <param name="handler">Обработчик, принимающий идентификатор актёра и ссылку на удаляемый компонент.</param>
    public void OnComponentRemoving<T>(ActorComponentRemoving<T> handler)
        where T : struct, IActorComponent
    {
        var pool = GetOrCreateComponentPool<T>();
        pool.ComponentRemoving += handler;
    }

    /// <summary>
    /// Регистрирует обработчик, который будет вызван перед обновлением компонента типа <typeparamref name="T"/> у любого актёра.
    /// </summary>
    /// <typeparam name="T">Тип компонента. Должен быть структурой и реализовывать <see cref="IActorComponent"/>.</typeparam>
    /// <param name="handler">Обработчик, принимающий идентификатор актёра и ссылки на старое и новое состояния компонента.</param>
    public void OnComponentUpdating<T>(ActorComponentUpdating<T> handler)
        where T : struct, IActorComponent
    {
        var pool = GetOrCreateComponentPool<T>();
        pool.ComponentUpdating += handler;
    }

    /// <summary>
    /// Удаляет компонент указанного типа у заданного актёра.
    /// </summary>
    /// <typeparam name="T">Тип компонента. Должен быть структурой и реализовывать <see cref="IActorComponent"/>.</typeparam>
    /// <param name="actorId">Идентификатор актёра.</param>
    /// <returns><c>true</c>, если компонент был успешно удален; в противном случае <c>false</c> (например, если компонент не найден).</returns>
    public bool RemoveComponent<T>(uint actorId)
        where T : struct, IActorComponent
    {
        var pool = GetComponentPool<T>();
        if (pool == null || !pool.Remove(actorId)) return false;

        ref var entry = ref GetEntryExact(actorId);
        entry.Remove(ActorComponentType<T>.Id);

        return true;
    }

    /// <summary>
    /// Удаляет компонент указанного типа у заданного актёра и возвращает его значение.
    /// </summary>
    /// <typeparam name="T">Тип компонента. Должен быть структурой и реализовывать <see cref="IActorComponent"/>.</typeparam>
    /// <param name="actorId">Идентификатор актёра.</param>
    /// <param name="component">Удаленный компонент.</param>
    /// <returns><c>true</c>, если компонент был успешно удален; в противном случае <c>false</c>.</returns>
    public bool RemoveComponent<T>(uint actorId, out T component)
        where T : struct, IActorComponent
    {
        var pool = GetComponentPool<T>();
        if (pool == null || !pool.Remove(actorId, out component))
        {
            component = default;
            return false;
        }

        ref var entry = ref GetEntryExact(actorId);
        entry.Remove(ActorComponentType<T>.Id);

        return true;
    }

    /// <summary>
    /// Пытается добавить компонент к указанному актёру.
    /// </summary>
    /// <typeparam name="T">Тип компонента. Должен быть структурой и реализовывать <see cref="IActorComponent"/>.</typeparam>
    /// <param name="actorId">Идентификатор актёра.</param>
    /// <param name="component">Компонент для добавления.</param>
    /// <returns><c>true</c>, если компонент успешно добавлен; <c>false</c>, если актёр уже имеет компонент данного типа.</returns>
    public bool TryAdd<T>(uint actorId, in T component)
        where T : struct, IActorComponent
    {
        var pool = GetOrCreateComponentPool<T>();
        var result = pool.TryAdd(actorId, in component);

        if (!result) return false;

        ref var entry = ref GetEntryExact(actorId);
        entry.Add(ActorComponentType<T>.Id);
        return true;
    }

    /// <summary>
    /// Пытается получить ссылку на компонент указанного типа для заданного актёра.
    /// </summary>
    /// <typeparam name="T">Тип компонента. Должен быть структурой и реализовывать <see cref="IActorComponent"/>.</typeparam>
    /// <param name="actorId">Идентификатор актёра.</param>
    /// <returns>Ссылка на компонент, если он существует; в противном случае <see cref="Unsafe.NullRef{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T TryGetComponentRef<T>(uint actorId)
        where T : struct, IActorComponent
    {
        var pool = GetComponentPool<T>();
        if (pool != null)
        {
            return ref pool.TryGet(actorId);
        }

        return ref Unsafe.NullRef<T>();
    }

    /// <summary>
    /// Обновляет компонент указанного типа у заданного актёра.
    /// </summary>
    /// <typeparam name="T">Тип компонента. Должен быть структурой и реализовывать <see cref="IActorComponent"/>.</typeparam>
    /// <param name="actorId">Идентификатор актёра.</param>
    /// <param name="component">Новое значение компонента.</param>
    /// <param name="createIfNotExists">Если <c>true</c>, компонент будет добавлен, если он не существует. По умолчанию <c>true</c>.</param>
    /// <returns><c>true</c>, если компонент был обновлен или добавлен; <c>false</c>, если пул компонентов не найден или <paramref name="createIfNotExists"/> равен <c>false</c> и компонент не существует.</returns>
    public bool UpdateComponent<T>(uint actorId, in T component, bool createIfNotExists = true)
        where T : struct, IActorComponent
    {
        var pool = GetComponentPool<T>();
        if (pool == null) return false;

        if (pool.Update(actorId, in component)) return true;

        if (!createIfNotExists) return false;

        AddComponent(actorId, in component);
        return true;
    }

    /// <summary>
    /// Получает пул компонентов определенного типа.
    /// </summary>
    /// <typeparam name="T">Тип компонента актера.</typeparam>
    /// <returns>
    /// Пул компонентов указанного типа или null, если пул не существует.
    /// </returns>
    /// <remarks>
    /// Метод выполняет быструю проверку наличия пула компонентов по идентификатору типа.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ActorComponentPool<T>? GetComponentPool<T>() where T : struct, IActorComponent
    {
        var id = ActorComponentType<T>.Id;

        var pools = _componentPools;

        if (id >= pools.Length) return null;
        var pool = pools[id];

        return pool == null
            ? null
            : Unsafe.As<ActorComponentPool<T>>(pool);
    }

    /// <summary>
    /// Получает существующий пул компонентов указанного типа или создает новый, если он не существует.
    /// </summary>
    /// <typeparam name="T">Тип компонента актера.</typeparam>
    /// <returns>
    /// Пул компонентов указанного типа (существующий или вновь созданный).
    /// </returns>
    internal ActorComponentPool<T> GetOrCreateComponentPool<T>() where T : struct, IActorComponent
    {
        var id = ActorComponentType<T>.Id;
        if (id < _componentPools.Length)
        {
            var existsPool = _componentPools[id];
            if (existsPool != null) return Unsafe.As<ActorComponentPool<T>>(existsPool);
        }

#if NET9_0_OR_GREATER
        using (_componentPoolLock.EnterScope())
#else
        lock (_componentPoolLock)
#endif
        {
            ArrayUtils.EnsureCapacity(ref _componentPools, id);
            ref var pool = ref _componentPools[id];
            pool ??= new ActorComponentPool<T>(this, GetOrCreateComponentConfiguration<T>());

            return Unsafe.As<ActorComponentPool<T>>(pool);
        }
    }

    private ActorComponentConfiguration<T> GetOrCreateComponentConfiguration<T>()
        where T : struct, IActorComponent
    {
        var id = ActorComponentType<T>.Id;

        if (id >= _componentConfigurations.Length) return ActorComponentConfiguration<T>.Empty;

        var existsConfiguration = _componentConfigurations[id];
        return existsConfiguration == null
            ? ActorComponentConfiguration<T>.Empty
            : Unsafe.As<ActorComponentConfiguration<T>>(existsConfiguration);
    }
}