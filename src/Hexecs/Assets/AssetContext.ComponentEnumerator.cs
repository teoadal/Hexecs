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
            get => new ComponentEnumerator();
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
        private readonly AssetId _assetId;
        private readonly ReadOnlySpan<ushort> _componentIds;
        private readonly IAssetComponentPool?[] _pools;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ComponentEnumerator()
        {
            _index = -1;
            _assetId = AssetId.Empty;
            _componentIds = ReadOnlySpan<ushort>.Empty;
            _pools = [];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ComponentEnumerator(AssetId assetId, IAssetComponentPool?[] pools, ReadOnlySpan<ushort> componentIds)
        {
            _index = -1;
            _assetId = assetId;
            _componentIds = componentIds;
            _pools = pools;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            return ++_index < _componentIds.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ComponentEnumerator GetEnumerator()
        {
            return this;
        }

        public readonly IAssetComponent[] ToArray()
        {
            ReadOnlySpan<ushort> ids = _componentIds;

            if (ids.Length == 0)
            {
                return [];
            }

            IAssetComponent[] array = ArrayUtils.Create<IAssetComponent>(ids.Length);
            for (var i = 0; i < ids.Length; i++)
            {
                array[i] = _pools[ids[i]]!.Get(_assetId);
            }

            return array;
        }
    }
}