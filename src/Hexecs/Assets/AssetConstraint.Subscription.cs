using Hexecs.Assets.Components;

namespace Hexecs.Assets;

public sealed partial class AssetConstraint
{
    private readonly struct Subscription : IComparable<Subscription>, IEquatable<Subscription>
    {
        public ushort ComponentId
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _pool.Id;
        }

        public readonly Func<AssetId, bool> Check;

        private readonly bool _include;
        private readonly IAssetComponentPool _pool;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Subscription(bool include, IAssetComponentPool pool, Func<AssetId, bool> check)
        {
            Check = check;

            _include = include;
            _pool = pool;
        }

        #region Equality

        public int CompareTo(Subscription other)
        {
            int componentIdComparison = _pool.Id.CompareTo(other._pool.Id);
            return componentIdComparison != 0
                ? componentIdComparison
                : _include.CompareTo(other._include);
        }

        public bool Equals(Subscription other)
        {
            return _include == other._include &&
                _pool.Id == other._pool.Id;
        }

        public override bool Equals(object? obj)
        {
            return obj is Subscription other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_pool.Id, _include ? 2 : 3);
        }

        #endregion
    }
}