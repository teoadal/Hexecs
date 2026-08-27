using System.Collections.Frozen;

namespace Hexecs.Assets;

public sealed partial class AssetFilter<T1>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator() => new(this);

    public ref struct Enumerator
    {
        public readonly AssetRef<T1> Current
        {
            get
            {
                var filter = _filter;
                var (assetId, entry) = _enumerator.Current;
                return new AssetRef<T1>(
                    filter.Context,
                    new AssetId(assetId),
                    ref filter._pool1.GetByIndex(entry.Index1));
            }
        }

        public readonly int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _filter.Length;
        }

        private readonly AssetFilter<T1> _filter;
        private FrozenDictionary<uint, Entry>.Enumerator _enumerator;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(AssetFilter<T1> filter)
        {
            _filter = filter;
            _enumerator = filter._dictionary.GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() => _enumerator.MoveNext();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Enumerator GetEnumerator() => this;
    }
}