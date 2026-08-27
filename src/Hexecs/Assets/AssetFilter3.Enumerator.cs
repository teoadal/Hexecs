using System.Collections.Frozen;

namespace Hexecs.Assets;

public sealed partial class AssetFilter<T1, T2, T3>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    public ref struct Enumerator
    {
        public readonly AssetRef<T1, T2, T3> Current
        {
            get
            {
                AssetFilter<T1, T2, T3> filter = _filter;
                (uint assetId, Entry entry) = _enumerator.Current;
                return new AssetRef<T1, T2, T3>(
                    filter.Context,
                    new AssetId(assetId),
                    ref filter._pool1.GetByIndex(entry.Index1),
                    ref filter._pool2.GetByIndex(entry.Index2),
                    ref filter._pool3.GetByIndex(entry.Index3));
            }
        }
        
        public readonly int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _filter.Length;
        }

        private readonly AssetFilter<T1, T2, T3> _filter;
        private FrozenDictionary<uint, Entry>.Enumerator _enumerator;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(AssetFilter<T1, T2, T3> filter)
        {
            _filter = filter;
            _enumerator = filter._dictionary.GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            return _enumerator.MoveNext();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Enumerator GetEnumerator()
        {
            return this;
        }

        public Asset[] ToArray()
        {
            var count = 0;
            Asset[] assets = ArrayUtils.Create<Asset>(_filter.Length);
            foreach (AssetRef<T1, T2, T3> asset in this)
            {
                assets[count++] = asset;
            }

            return assets;
        }
    }
}