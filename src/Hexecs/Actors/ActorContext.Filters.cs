namespace Hexecs.Actors;

public sealed partial class ActorContext
{
    private ActorConstraint.Builder? _constraintBuilder;
    private readonly Dictionary<Type, IActorFilter> _filters;
    private readonly List<IActorFilter> _filtersWithConstraint;

    #region Filter1

    /// <summary>
    /// Получает или создает фильтр актёров для указанного типа компонента.
    /// </summary>
    /// <typeparam name="T1">Тип компонента, который должен быть структурой и реализовывать <see cref="IActorComponent"/>.</typeparam>
    /// <returns>Экземпляр <see cref="ActorFilter{T1}"/>.</returns>
    /// <remarks>
    /// Если фильтр для данного типа компонента уже существует, возвращается существующий экземпляр.
    /// В противном случае создается новый фильтр и добавляется в коллекцию.
    /// </remarks>
    public ActorFilter<T1> Filter<T1>()
        where T1 : struct, IActorComponent
    {
        var key = typeof(ActorFilter<T1>);

        // ReSharper disable once InvertIf
        if (!_filters.TryGetValue(key, out var collection))
        {
            collection = new ActorFilter<T1>(this);
            _filters.Add(key, collection);
        }

        return Unsafe.As<ActorFilter<T1>>(collection);
    }

    /// <summary>
    /// Получает или создает фильтр актёров для указанного типа компонента с заданными ограничениями.
    /// </summary>
    /// <typeparam name="T1">Тип компонента, который должен быть структурой и реализовывать <see cref="IActorComponent"/>.</typeparam>
    /// <param name="constraint">Действие для настройки ограничений фильтра.</param>
    /// <returns>Экземпляр <see cref="ActorFilter{T1}"/> с примененными ограничениями.</returns>
    /// <remarks>
    /// Если фильтр для данного типа компонента и набора ограничений уже существует, возвращается существующий экземпляр.
    /// В противном случае создается новый фильтр с указанными ограничениями и добавляется в коллекцию.
    /// Любые исключения, вызванные делегатом <paramref name="constraint"/>, будут распространяться из этого метода.
    /// </remarks>
    public ActorFilter<T1> Filter<T1>(Action<ActorConstraint.Builder> constraint)
        where T1 : struct, IActorComponent
    {
        var builder = ResolveConstraintBuilder();
        constraint(builder);

        foreach (var filter in _filtersWithConstraint)
        {
            // ReSharper disable once InvertIf
            if (filter is ActorFilter<T1> expected && builder.Equals(filter.Constraint))
            {
                ReturnConstraintBuilder(builder);
                return expected;
            }
        }

        var newFilter = new ActorFilter<T1>(this, FlushConstraintBuilder(builder));
        _filtersWithConstraint.Add(newFilter);
        return newFilter;
    }

    #endregion

    #region Filter2

    /// <summary>
    /// Получает или создает фильтр актёров для указанных типов компонентов.
    /// </summary>
    /// <typeparam name="T1">Первый тип компонента, который должен быть структурой и реализовывать <see cref="IActorComponent"/>.</typeparam>
    /// <typeparam name="T2">Второй тип компонента, который должен быть структурой и реализовывать <see cref="IActorComponent"/>.</typeparam>
    /// <returns>Экземпляр <see cref="ActorFilter{T1, T2}"/>.</returns>
    /// <remarks>
    /// Если фильтр для данной комбинации типов компонентов уже существует, возвращается существующий экземпляр.
    /// В противном случае создается новый фильтр и добавляется в коллекцию.
    /// </remarks>
    public ActorFilter<T1, T2> Filter<T1, T2>()
        where T1 : struct, IActorComponent
        where T2 : struct, IActorComponent
    {
        var key = typeof(ActorFilter<T1, T2>);

        // ReSharper disable once InvertIf
        if (!_filters.TryGetValue(key, out var collection))
        {
            collection = new ActorFilter<T1, T2>(this);
            _filters.Add(key, collection);
        }

        return Unsafe.As<ActorFilter<T1, T2>>(collection);
    }

    /// <summary>
    /// Получает или создает фильтр актёров для указанных типов компонентов с заданными ограничениями.
    /// </summary>
    /// <typeparam name="T1">Первый тип компонента, который должен быть структурой и реализовывать <see cref="IActorComponent"/>.</typeparam>
    /// <typeparam name="T2">Второй тип компонента, который должен быть структурой и реализовывать <see cref="IActorComponent"/>.</typeparam>
    /// <param name="constraint">Действие для настройки ограничений фильтра.</param>
    /// <returns>Экземпляр <see cref="ActorFilter{T1, T2}"/> с примененными ограничениями.</returns>
    /// <remarks>
    /// Если фильтр для данной комбинации типов компонентов и набора ограничений уже существует, возвращается существующий экземпляр.
    /// В противном случае создается новый фильтр с указанными ограничениями и добавляется в коллекцию.
    /// Любые исключения, вызванные делегатом <paramref name="constraint"/>, будут распространяться из этого метода.
    /// </remarks>
    public ActorFilter<T1, T2> Filter<T1, T2>(Action<ActorConstraint.Builder> constraint)
        where T1 : struct, IActorComponent
        where T2 : struct, IActorComponent
    {
        var builder = ResolveConstraintBuilder();
        constraint(builder);

        foreach (var filter in _filtersWithConstraint)
        {
            // ReSharper disable once InvertIf
            if (filter is ActorFilter<T1, T2> expected && builder.Equals(filter.Constraint))
            {
                ReturnConstraintBuilder(builder);
                return expected;
            }
        }

        var newFilter = new ActorFilter<T1, T2>(this, FlushConstraintBuilder(builder));
        _filtersWithConstraint.Add(newFilter);
        return newFilter;
    }

    #endregion

    #region Filter3

    /// <summary>
    /// Получает или создает фильтр актёров для указанных типов компонентов.
    /// </summary>
    /// <typeparam name="T1">Первый тип компонента, который должен быть структурой и реализовывать <see cref="IActorComponent"/>.</typeparam>
    /// <typeparam name="T2">Второй тип компонента, который должен быть структурой и реализовывать <see cref="IActorComponent"/>.</typeparam>
    /// <typeparam name="T3">Третий тип компонента, который должен быть структурой и реализовывать <see cref="IActorComponent"/>.</typeparam>
    /// <returns>Экземпляр <see cref="ActorFilter{T1, T2, T3}"/>.</returns>
    /// <remarks>
    /// Если фильтр для данной комбинации типов компонентов уже существует, возвращается существующий экземпляр.
    /// В противном случае создается новый фильтр и добавляется в коллекцию.
    /// </remarks>
    public ActorFilter<T1, T2, T3> Filter<T1, T2, T3>()
        where T1 : struct, IActorComponent
        where T2 : struct, IActorComponent
        where T3 : struct, IActorComponent
    {
        var key = typeof(ActorFilter<T1, T2, T3>);

        // ReSharper disable once InvertIf
        if (!_filters.TryGetValue(key, out var collection))
        {
            collection = new ActorFilter<T1, T2, T3>(this);
            _filters.Add(key, collection);
        }

        return Unsafe.As<ActorFilter<T1, T2, T3>>(collection);
    }

    /// <summary>
    /// Получает или создает фильтр актёров для указанных типов компонентов с заданными ограничениями.
    /// </summary>
    /// <typeparam name="T1">Первый тип компонента, который должен быть структурой и реализовывать <see cref="IActorComponent"/>.</typeparam>
    /// <typeparam name="T2">Второй тип компонента, который должен быть структурой и реализовывать <see cref="IActorComponent"/>.</typeparam>
    /// <typeparam name="T3">Третий тип компонента, который должен быть структурой и реализовывать <see cref="IActorComponent"/>.</typeparam>
    /// <param name="constraint">Действие для настройки ограничений фильтра.</param>
    /// <returns>Экземпляр <see cref="ActorFilter{T1, T2, T3}"/> с примененными ограничениями.</returns>
    /// <remarks>
    /// Если фильтр для данной комбинации типов компонентов и набора ограничений уже существует, возвращается существующий экземпляр.
    /// В противном случае создается новый фильтр с указанными ограничениями и добавляется в коллекцию.
    /// Любые исключения, вызванные делегатом <paramref name="constraint"/>, будут распространяться из этого метода.
    /// </remarks>
    public ActorFilter<T1, T2, T3> Filter<T1, T2, T3>(Action<ActorConstraint.Builder> constraint)
        where T1 : struct, IActorComponent
        where T2 : struct, IActorComponent
        where T3 : struct, IActorComponent
    {
        var builder = ResolveConstraintBuilder();
        constraint(builder);

        foreach (var filter in _filtersWithConstraint)
        {
            // ReSharper disable once InvertIf
            if (filter is ActorFilter<T1, T2, T3> expected && builder.Equals(filter.Constraint))
            {
                ReturnConstraintBuilder(builder);
                return expected;
            }
        }

        var newFilter = new ActorFilter<T1, T2, T3>(this, FlushConstraintBuilder(builder));
        _filtersWithConstraint.Add(newFilter);
        return newFilter;
    }

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ActorConstraint FlushConstraintBuilder(ActorConstraint.Builder builder)
    {
        var constraint = builder.Build();
        ReturnConstraintBuilder(builder);

        return constraint;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReturnConstraintBuilder(ActorConstraint.Builder builder)
    {
        builder.Clear();
        Interlocked.Exchange(ref _constraintBuilder, builder);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ActorConstraint.Builder ResolveConstraintBuilder()
    {
        return Interlocked.Exchange(ref _constraintBuilder, null) ?? new ActorConstraint.Builder(this);
    }
}