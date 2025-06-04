namespace Hexecs.Assets;

public sealed partial class AssetConstraint : IEquatable<AssetConstraint>
{
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