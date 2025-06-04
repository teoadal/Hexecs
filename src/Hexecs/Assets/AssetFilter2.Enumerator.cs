using System.Collections.Frozen;

namespace Hexecs.Assets;

public sealed partial class AssetFilter<T1, T2>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator() => new(this);

    public ref struct Enumerator
    {
        public readonly AssetRef<T1, T2> Current
        {
            get
            {
                var filter = _filter;
                var (actorId, entry) = _enumerator.Current;
                return new AssetRef<T1, T2>(
                    filter.Context,
                    actorId,
                    ref filter._pool1.GetByIndex(entry.Index1),
                    ref filter._pool2.GetByIndex(entry.Index2));
            }
        }
        
        public readonly int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _filter.Length;
        }

        private readonly AssetFilter<T1, T2> _filter;
        private FrozenDictionary<uint, Entry>.Enumerator _enumerator;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(AssetFilter<T1, T2> filter)
        {
            _filter = filter;
            _enumerator = filter._dictionary.GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() => _enumerator.MoveNext();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Enumerator GetEnumerator() => this;
        
        public Asset[] ToArray()
        {
            var count = 0;
            var assets = ArrayUtils.Create<Asset>(_filter.Length);
            foreach (var asset in this)
            {
                assets[count++] = asset;
            }

            return assets;
        }
    }
}