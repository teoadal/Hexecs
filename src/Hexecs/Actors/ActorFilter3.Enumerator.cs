using Hexecs.Actors.Components;

namespace Hexecs.Actors;

public sealed partial class ActorFilter<T1, T2, T3>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator()
    {
#if NET9_0_OR_GREATER
        using (_postponedSyncLock.EnterScope())
#else
        lock (_postponedSyncLock)
#endif
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

        private readonly ReadOnlySpan<uint> _ids;
        private int _index;

        public readonly ActorRef<T1, T2, T3> Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var id = _ids[_index];

                return new ActorRef<T1, T2, T3>(
                    _context,
                    id,
                    ref _pool1.Get(id),
                    ref _pool2.Get(id),
                    ref _pool3.Get(id));
            }
        }

        public readonly int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _ids.Length;
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
            _ids = filter._dense.AsSpan(0, count);

            _index = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _filter.ProcessPostponedUpdates();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() => ++_index < _ids.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Enumerator GetEnumerator() => this;
    }
}