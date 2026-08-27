namespace Hexecs.Actors;

public sealed partial class ActorFilter<T1>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SkipTakeEnumerator Skip(int skip, int take = int.MaxValue)
    {
#if NET9_0_OR_GREATER
        using (_postponedSyncLock.EnterScope())
#else
        lock (_postponedSyncLock)
#endif
        {
            Interlocked.Increment(ref _postponedReadersCount);
        }

        return new SkipTakeEnumerator(this, skip, take);
    }

    public ref struct SkipTakeEnumerator
    {
        public readonly ActorRef<T1> Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                uint id = _ids[_index];

                return new ActorRef<T1>(
                    _context,
                    new ActorId(id),
                    ref _pool1[id]);
            }
        }

        public readonly int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _filter.Length;
        }

        private readonly ActorContext _context;
        private readonly ActorFilter<T1> _filter;
        private readonly ComponentsAccess<T1> _pool1;

        private readonly ReadOnlySpan<uint> _ids;
        private int _index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal SkipTakeEnumerator(ActorFilter<T1> filter, int skip, int take = int.MaxValue)
        {
            _context = filter.Context;
            _filter = filter;
            _pool1 = filter._pool1.GetComponentAccess();

            int count = filter._count;
            int actualSkip = Math.Min(skip, count);
            int actualTake = Math.Min(take, count - actualSkip);

            _ids = filter._dense.AsSpan(actualSkip, actualTake);
            _index = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _filter.ProcessPostponedUpdates();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            return ++_index < _ids.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly SkipTakeEnumerator GetEnumerator()
        {
            return this;
        }
    }
}
