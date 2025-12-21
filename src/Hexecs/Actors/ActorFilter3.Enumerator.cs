using Hexecs.Actors.Components;

namespace Hexecs.Actors;

public sealed partial class ActorFilter<T1, T2, T3>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator()
    {
        using (_postponedSyncLock.EnterScope())
        {
            Interlocked.Increment(ref _postponedReadersCount);
        }

        return new Enumerator(this);
    }

    public ref struct Enumerator
    {
        private readonly ActorContext _context;
        private readonly ActorFilter<T1, T2, T3> _filter;
        private readonly ActorComponentPool<T1> _pool1;
        private readonly ActorComponentPool<T2> _pool2;
        private readonly ActorComponentPool<T3> _pool3;

        private readonly ReadOnlySpan<uint> _keys;
        private readonly ReadOnlySpan<Entry> _entries;

        private int _index;
        
        public readonly ActorRef<T1, T2, T3> Current
        {
            get
            {
                var index = _index;
                ref readonly var entry = ref _entries[index];

                return new ActorRef<T1, T2, T3>(
                    _context,
                    _keys[index],
                    ref _pool1.GetByIndex(entry.Index1),
                    ref _pool2.GetByIndex(entry.Index2),
                    ref _pool3.GetByIndex(entry.Index3));
            }
        }

        public readonly int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _keys.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(ActorFilter<T1, T2, T3> filter)
        {
            _context = filter.Context;
            _filter = filter;
            _pool1 = filter._pool1;
            _pool2 = filter._pool2;
            _pool3 = filter._pool3;

            var count = filter._count;
            _keys = filter._dense.AsSpan(0, count);
            _entries = filter._values.AsSpan(0, count);

            _index = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _filter.ProcessPostponedUpdates();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() => ++_index < _keys.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Enumerator GetEnumerator() => this;
    }
}