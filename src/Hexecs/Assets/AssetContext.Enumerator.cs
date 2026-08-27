namespace Hexecs.Assets;

public sealed partial class AssetContext
{
    /// <summary>
    /// Перечислитель ассетов контекста
    /// </summary>
    [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
    public struct Enumerator : IEnumerator<Asset>, IEnumerable<Asset>
    {
        public readonly Asset Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new Asset(_context, new AssetId(_enumerator.Current));
        }

        public readonly int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _context.Length;
        }

        private readonly AssetContext _context;
        private Dictionary<uint, Entry>.KeyCollection.Enumerator _enumerator;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(AssetContext context)
        {
            _context = context;
            _enumerator = context._entries.Keys.GetEnumerator();
        }

        public void Dispose()
        {
            _enumerator.Dispose();
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

        #region Interfaces

        readonly object IEnumerator.Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Current;
        }

        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        readonly IEnumerator<Asset> IEnumerable<Asset>.GetEnumerator()
        {
            return this;
        }

        readonly void IEnumerator.Reset()
        {
        }

        #endregion
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator<Asset> IEnumerable<Asset>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}