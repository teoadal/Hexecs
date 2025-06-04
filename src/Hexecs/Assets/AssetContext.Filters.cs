namespace Hexecs.Assets;

public sealed partial class AssetContext
{
    private AssetConstraint.Builder? _constraintBuilder;
    private readonly Dictionary<Type, IAssetFilter> _filters;
    private readonly List<IAssetFilter> _filtersWithConstraint;

    #region Filter1

    /// <summary>
    /// Создает или возвращает существующий фильтр ассетов для компонентов одного типа.
    /// </summary>
    /// <typeparam name="T1">Тип компонента ассета</typeparam>
    /// <returns>Экземпляр фильтра ассетов</returns>
    public AssetFilter<T1> Filter<T1>()
        where T1 : struct, IAssetComponent
    {
        var key = typeof(AssetFilter<T1>);

        // ReSharper disable once InvertIf
        if (!_filters.TryGetValue(key, out var collection))
        {
            collection = new AssetFilter<T1>(this);
            _filters.Add(key, collection);
        }

        return Unsafe.As<AssetFilter<T1>>(collection);
    }

    /// <summary>
    /// Создает или возвращает существующий фильтр ассетов для компонентов одного типа
    /// с применением дополнительных ограничений.
    /// </summary>
    /// <typeparam name="T1">Тип компонента ассета</typeparam>
    /// <param name="constraint">Делегат для настройки ограничений фильтра</param>
    /// <returns>Экземпляр фильтра ассетов с заданными ограничениями</returns>
    public AssetFilter<T1> Filter<T1>(Action<AssetConstraint.Builder> constraint)
        where T1 : struct, IAssetComponent
    {
        var builder = ResolveConstraintBuilder();
        constraint(builder);

        foreach (var filter in _filtersWithConstraint)
        {
            // ReSharper disable once InvertIf
            if (filter is AssetFilter<T1> expected && builder.Equals(filter.Constraint))
            {
                ReturnConstraintBuilder(builder);
                return expected;
            }
        }

        var newFilter = new AssetFilter<T1>(this, FlushConstraintBuilder(builder));
        _filtersWithConstraint.Add(newFilter);
        return newFilter;
    }

    #endregion

    #region Filter2

    /// <summary>
    /// Создает или возвращает существующий фильтр ассетов для компонентов двух типов.
    /// </summary>
    /// <typeparam name="T1">Первый тип компонента ассета</typeparam>
    /// <typeparam name="T2">Второй тип компонента ассета</typeparam>
    /// <returns>Экземпляр фильтра ассетов</returns>
    public AssetFilter<T1, T2> Filter<T1, T2>()
        where T1 : struct, IAssetComponent
        where T2 : struct, IAssetComponent
    {
        var key = typeof(AssetFilter<T1, T2>);

        // ReSharper disable once InvertIf
        if (!_filters.TryGetValue(key, out var collection))
        {
            collection = new AssetFilter<T1, T2>(this);
            _filters.Add(key, collection);
        }

        return Unsafe.As<AssetFilter<T1, T2>>(collection);
    }

    /// <summary>
    /// Создает или возвращает существующий фильтр активов для компонентов двух типов
    /// с применением дополнительных ограничений.
    /// </summary>
    /// <typeparam name="T1">Первый тип компонента ассета</typeparam>
    /// <typeparam name="T2">Второй тип компонента ассета</typeparam>
    /// <param name="constraint">Делегат для настройки ограничений фильтра</param>
    /// <returns>Экземпляр фильтра ассетов с заданными ограничениями</returns>
    public AssetFilter<T1, T2> Filter<T1, T2>(Action<AssetConstraint.Builder> constraint)
        where T1 : struct, IAssetComponent
        where T2 : struct, IAssetComponent
    {
        var builder = ResolveConstraintBuilder();
        constraint(builder);

        foreach (var filter in _filtersWithConstraint)
        {
            // ReSharper disable once InvertIf
            if (filter is AssetFilter<T1, T2> expected && builder.Equals(filter.Constraint))
            {
                ReturnConstraintBuilder(builder);
                return expected;
            }
        }

        var newFilter = new AssetFilter<T1, T2>(this, FlushConstraintBuilder(builder));
        _filtersWithConstraint.Add(newFilter);
        return newFilter;
    }

    #endregion

    #region Filter3

    /// <summary>
    /// Создает или возвращает существующий фильтр ассетов для компонентов трех типов.
    /// </summary>
    /// <typeparam name="T1">Первый тип компонента ассета</typeparam>
    /// <typeparam name="T2">Второй тип компонента ассета</typeparam>
    /// <typeparam name="T3">Третий тип компонента ассета</typeparam>
    /// <returns>Экземпляр фильтра ассетов</returns>
    public AssetFilter<T1, T2, T3> Filter<T1, T2, T3>()
        where T1 : struct, IAssetComponent
        where T2 : struct, IAssetComponent
        where T3 : struct, IAssetComponent
    {
        var key = typeof(AssetFilter<T1, T2, T3>);

        // ReSharper disable once InvertIf
        if (!_filters.TryGetValue(key, out var collection))
        {
            collection = new AssetFilter<T1, T2, T3>(this);
            _filters.Add(key, collection);
        }

        return Unsafe.As<AssetFilter<T1, T2, T3>>(collection);
    }

    /// <summary>
    /// Создает или возвращает существующий фильтр ассетов для компонентов трех типов
    /// с применением дополнительных ограничений.
    /// </summary>
    /// <typeparam name="T1">Первый тип компонента ассета</typeparam>
    /// <typeparam name="T2">Второй тип компонента ассета</typeparam>
    /// <typeparam name="T3">Третий тип компонента ассета</typeparam>
    /// <param name="constraint">Делегат для настройки ограничений фильтра</param>
    /// <returns>Экземпляр фильтра ассетов с заданными ограничениями</returns>
    public AssetFilter<T1, T2, T3> Filter<T1, T2, T3>(Action<AssetConstraint.Builder> constraint)
        where T1 : struct, IAssetComponent
        where T2 : struct, IAssetComponent
        where T3 : struct, IAssetComponent
    {
        var builder = ResolveConstraintBuilder();
        constraint(builder);

        foreach (var filter in _filtersWithConstraint)
        {
            // ReSharper disable once InvertIf
            if (filter is AssetFilter<T1, T2, T3> expected && builder.Equals(filter.Constraint))
            {
                ReturnConstraintBuilder(builder);
                return expected;
            }
        }

        var newFilter = new AssetFilter<T1, T2, T3>(this, FlushConstraintBuilder(builder));
        _filtersWithConstraint.Add(newFilter);
        return newFilter;
    }

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private AssetConstraint FlushConstraintBuilder(AssetConstraint.Builder builder)
    {
        var constraint = builder.Build();
        ReturnConstraintBuilder(builder);

        return constraint;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReturnConstraintBuilder(AssetConstraint.Builder builder)
    {
        builder.Clear();
        Interlocked.Exchange(ref _constraintBuilder, builder);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private AssetConstraint.Builder ResolveConstraintBuilder()
    {
        return Interlocked.Exchange(ref _constraintBuilder, null) ?? new AssetConstraint.Builder(this);
    }
}