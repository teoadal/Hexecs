namespace Hexecs.Actors;

public sealed partial class ActorContext
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator() => new(this);

    IEnumerator<Actor> IEnumerable<Actor>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
    public struct Enumerator : IEnumerator<Actor>, IEnumerable<Actor>
    {
        public readonly Actor Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(_context, _dense[_index]);
        }

        public readonly int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count;
        }
        
        private int _index;
        private readonly ActorContext _context;
        private readonly uint[] _dense;
        private readonly int _count;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(ActorContext context)
        {
            _index = -1;
            _context = context;
            _dense = context._dense;
            _count = context._count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() => ++_index < _count;

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

        void IEnumerator.Reset()
        {
            _index = -1;
        }

        #endregion
    }
}