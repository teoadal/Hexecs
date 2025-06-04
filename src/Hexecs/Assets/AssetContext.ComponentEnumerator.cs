using Hexecs.Assets.Components;

namespace Hexecs.Assets;

public sealed partial class AssetContext
{
    /// <summary>
    /// Перечислитель для доступа к компонентам ассета
    /// </summary>
    public ref struct ComponentEnumerator
    {
        public static ComponentEnumerator Empty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new();
        }

        public readonly IAssetComponent Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _pools[_index]!.Get(_assetId);
        }

        public readonly int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _componentIds.Length;
        }

        private int _index;
        private readonly uint _assetId;
        private readonly ReadOnlySpan<ushort> _componentIds;
        private readonly IAssetComponentPool?[] _pools;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ComponentEnumerator()
        {
            _index = -1;
            _assetId = 0;
            _componentIds = ReadOnlySpan<ushort>.Empty;
            _pools = [];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ComponentEnumerator(uint assetId, IAssetComponentPool?[] pools, ReadOnlySpan<ushort> componentIds)
        {
            _index = -1;
            _assetId = assetId;
            _componentIds = componentIds;
            _pools = pools;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() => ++_index < _componentIds.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ComponentEnumerator GetEnumerator() => this;

        public readonly IAssetComponent[] ToArray()
        {
            var ids = _componentIds;

            if (ids.Length == 0) return [];

            var array = ArrayUtils.Create<IAssetComponent>(ids.Length);
            for (var i = 0; i < ids.Length; i++)
            {
                array[i] = _pools[ids[i]]!.Get(_assetId);
            }

            return array;
        }
    }
}