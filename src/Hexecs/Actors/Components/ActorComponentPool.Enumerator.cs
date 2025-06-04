namespace Hexecs.Actors.Components;

internal sealed partial class ActorComponentPool<T>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator() => new(this);

    public ref struct Enumerator
    {
        public readonly ActorRef<T> Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var index = _index;
                return new ActorRef<T>(
                    _context, 
                    _dense[index], 
                    ref _values[index]);
            }
        }
        
        public readonly int Index
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _index;
        }

        public readonly int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count;
        }
        
        // Сначала ссылки (8 байт)
        private readonly ActorContext _context;
        private readonly uint[] _dense;
        private readonly T[] _values;

        // Примитивы (4 байта)
        private readonly int _count;
        private int _index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(ActorComponentPool<T> pool)
        {
            _context = pool.Context;
            _dense = pool._dense;
            _values = pool._values;
            _count = pool._count;
            _index = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() => ++_index < _count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Enumerator GetEnumerator() => this;
    }
}