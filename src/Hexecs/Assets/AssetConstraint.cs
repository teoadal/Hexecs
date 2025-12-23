namespace Hexecs.Assets;

public sealed partial class AssetConstraint : IEquatable<AssetConstraint>
{
    /// <summary>
    /// Создает построитель ограничений с исключением указанного компонента.
    /// </summary>
    /// <typeparam name="T1">Тип компонента, который должен отсутствовать у ассета</typeparam>
    /// <param name="context">Контекст ассетов</param>
    /// <returns>Построитель ограничений</returns>
    public static Builder Exclude<T1>(AssetContext context) where T1 : struct, IAssetComponent
    {
        return new Builder(context).Exclude<T1>();
    }

    /// <summary>
    /// Создает построитель ограничений с включением указанного компонента.
    /// </summary>
    /// <typeparam name="T1">Тип компонента, который должен присутствовать у ассета</typeparam>
    /// <param name="context">Контекст ассетов</param>
    /// <returns>Построитель ограничений</returns>
    public static Builder Include<T1>(AssetContext context) where T1 : struct, IAssetComponent
    {
        return new Builder(context).Include<T1>();
    }
    
    /// <summary>
    /// Создает построитель ограничений с включением двух указанных компонентов.
    /// </summary>
    /// <typeparam name="T1">Первый тип компонента, который должен присутствовать у ассета</typeparam>
    /// <typeparam name="T2">Второй тип компонента, который должен присутствовать у ассета</typeparam>
    /// <param name="context">Контекст актёров</param>
    /// <returns>Построитель ограничений</returns>
    public static Builder Include<T1, T2>(AssetContext context)
        where T1 : struct, IAssetComponent
        where T2 : struct, IAssetComponent
    {
        return new Builder(context)
            .Include<T1>()
            .Include<T2>();
    }

    /// <summary>
    /// Создает построитель ограничений с включением трех указанных компонентов.
    /// </summary>
    /// <typeparam name="T1">Первый тип компонента, который должен присутствовать у ассета</typeparam>
    /// <typeparam name="T2">Второй тип компонента, который должен присутствовать у ассета</typeparam>
    /// <typeparam name="T3">Третий тип компонента, который должен присутствовать у ассета</typeparam>
    /// <param name="context">Контекст актёров</param>
    /// <returns>Построитель ограничений</returns>
    public static Builder Include<T1, T2, T3>(AssetContext context)
        where T1 : struct, IAssetComponent
        where T2 : struct, IAssetComponent
        where T3 : struct, IAssetComponent
    {
        return new Builder(context)
            .Include<T1>()
            .Include<T2>()
            .Include<T3>();
    }
    
    private readonly int _hash;
    private readonly Subscription[] _subscriptions;

    private AssetConstraint(int hash, Subscription[] subscriptions)
    {
        _hash = hash;
        _subscriptions = subscriptions;
    }

    public bool Applicable(uint assetId)
    {
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var subscription in _subscriptions)
        {
            if (!subscription.Check(assetId)) return false;
        }

        return true;
    }
    
    #region Equality

    public bool Equals(AssetConstraint? other)
    {
        if (other == null) return false;

        var otherSubscriptions = other._subscriptions;

        // ReSharper disable once LoopCanBeConvertedToQuery
        for (var i = 0; i < _subscriptions.Length; i++)
        {
            if (!_subscriptions[i].Equals(otherSubscriptions[i])) return false;
        }

        return true;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is AssetConstraint other && Equals(other);
    }

    public override int GetHashCode() => _hash;

    #endregion
}