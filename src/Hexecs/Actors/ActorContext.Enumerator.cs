namespace Hexecs.Actors;

public sealed partial class ActorContext
{
    [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
    public struct Enumerator : IEnumerator<Actor>, IEnumerable<Actor>
    {
        public readonly Actor Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(_context, _context._entries[_index].Key);
        }

        public readonly int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _context.Length;
        }
        
        private int _index;
        private readonly ActorContext _context;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(ActorContext context)
        {
            _index = -1;
            _context = context;
        }

        public bool MoveNext()
        {
            var index = _index + 1;
            var length = _context._length;
            var entries = _context._entries;
            while ((uint)index < (uint)length)
            {
                ref readonly var entry = ref entries[index];
                if (entry.Next >= -1)
                {
                    _index = index;
                    return true;
                }

                index++;
            }

            _index = length;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Enumerator GetEnumerator() => this;

        #region Interfaces

        readonly object IEnumerator.Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Current;
        }

        readonly void IDisposable.Dispose()
        {
        }

        readonly IEnumerator IEnumerable.GetEnumerator() => this;

        readonly IEnumerator<Actor> IEnumerable<Actor>.GetEnumerator() => this;

        readonly void IEnumerator.Reset()
        {
        }

        #endregion
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator() => new(this);

    IEnumerator<Actor> IEnumerable<Actor>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}