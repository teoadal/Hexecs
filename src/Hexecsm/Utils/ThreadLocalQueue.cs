namespace Hexecsm.Utils;

internal sealed class ThreadLocalQueue<T> : IDisposable
    where T : struct
{
    private readonly Lock _addQueueLock;
    private readonly List<LocalQueue> _queues;
    private readonly ThreadLocal<LocalQueue> _localQueue;
    private readonly int _localCapacity;

    private bool _disposed;

    public ThreadLocalQueue(int localCapacity)
    {
        _addQueueLock = new Lock();
        _queues = new List<LocalQueue>(12);
        _localCapacity = localCapacity;
        _localQueue = new ThreadLocal<LocalQueue>(CreateQueue);
    }

    public void Clear()
    {
        foreach (LocalQueue queue in _queues)
        {
            queue.Clear(true);
        }
    }

    [SkipLocalsInit]
    public void Enqueue(in T item)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ThreadLocalQueue<T>));

        LocalQueue localQueue = _localQueue.Value!;
        localQueue.Enqueue(item);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<LocalQueue> GetBatches()
    {
        return CollectionsMarshal.AsSpan(_queues);
    }

    private LocalQueue CreateQueue()
    {
        using (_addQueueLock.EnterScope())
        {
            var localQueue = new LocalQueue(_localCapacity);

            _queues.Add(localQueue);

            return localQueue;
        }
    }

    internal sealed class LocalQueue(int capacity)
    {
        private int _count;
        private T[] _data = ArrayUtils.Create<T>(capacity);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
        {
            return new Span<T>(_data, 0, _count);
        }

        public void Clear(bool clearValues = false)
        {
            _count = 0;

            if (!clearValues)
            {
                return;
            }

            ClearSlow();
        }

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(in T item)
        {
            if ((uint)_count < (uint)_data.Length)
            {
                _data[_count] = item;
                _count++;

                return;
            }

            EnqueueSlow(in item);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ClearSlow()
        {
            foreach (ref T value in AsSpan())
            {
                value = default;
            }
        }

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void EnqueueSlow(in T item)
        {
            ArrayUtils.Resize(ref _data, _data.Length * 2);

            _data[_count] = item;
            _count++;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        foreach (LocalQueue queue in _queues)
        {
            queue.Clear(true);
        }

        _queues.Clear();
        _localQueue.Dispose();
    }
}
