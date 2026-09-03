namespace Hexecsm.Utils;

/// <summary>
/// Высокопроизводительная Lock-Free MPSC очередь для ECS, оперирующая пакетами (блоками) значений.
/// Ультимативно оптимизирована под строгую фазовую модель с гарантированным полным вычитыванием данных за кадр.
/// </summary>
internal sealed class MpscBlockQueue<T> where T : struct
{
    private readonly int _blockCapacity;
    private readonly QueueBlock _initialBlock;

    private volatile QueueBlock _head;
    private volatile QueueBlock _tail;

    public MpscBlockQueue(int blockCapacity)
    {
        _blockCapacity = blockCapacity;

        _initialBlock = new QueueBlock(_blockCapacity);
        _head = _initialBlock;
        _tail = _initialBlock;
    }

    /// <summary>
    /// Очищает и сбрасывает всю цепочку блоков для повторного использования в новом кадре.
    /// Вызывается в главном потоке ДО запуска воркеров.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        // Возвращаем голову в самое начало цепочки
        _head = _initialBlock;

        // Сбрасываем текущий хвост. Воркеры начнут писать с самого первого блока.
        _tail = _initialBlock;

        // Линейно обнуляем индексы блоков, которые успели нарасти.
        // Поскольку они уже в памяти (L1/L2 кэш), этот сброс происходит мгновенно.
        QueueBlock? current = _initialBlock;

        while (current is not null)
        {
            current.Reset();
            current = current.Next;
        }
    }

    /// <summary>
    /// Добавить элемент в очередь из любого потока-воркера (Multi-Producer).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Enqueue(in T item)
    {
        while (true)
        {
            QueueBlock currentTail = _tail;

            if (currentTail.TryEnqueue(in item))
            {
                return;
            }

            if (currentTail.Next is null)
            {
                var newBlock = new QueueBlock(_blockCapacity);

                if (Interlocked.CompareExchange(ref currentTail.Next, newBlock, null) != null)
                {
                    // Если другой воркер успел привязать блок — наш newBlock отпустит GC.
                    // Это редкая аллокация, происходящая только при пиковых нагрузках (росте пула).
                }
            }

            Interlocked.CompareExchange(ref _tail, currentTail.Next, currentTail);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public QueueEnumerator GetEnumerator()
    {
        return new QueueEnumerator(this);
    }

    public ref struct QueueEnumerator
    {
        private readonly MpscBlockQueue<T> _queue;
        private QueueBlock _currentBlock;
        private ReadOnlySpan<T> _currentSpan;
        private bool _isFirst;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal QueueEnumerator(MpscBlockQueue<T> queue)
        {
            _queue = queue;
            _currentBlock = queue._head;
            _currentSpan = ReadOnlySpan<T>.Empty;
            _isFirst = true;
        }

        public readonly ReadOnlySpan<T> Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _currentSpan;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            if (_isFirst)
            {
                _isFirst = false;

                return TryAdvanceBlock();
            }

            if (_currentBlock.IsFull && _currentBlock.Next is not null)
            {
                QueueBlock next = _currentBlock.Next;
                _queue._head = next;
                _currentBlock = next;

                return TryAdvanceBlock();
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryAdvanceBlock()
        {
            if (_currentBlock is null)
            {
                return false;
            }

            _currentSpan = _currentBlock.AsReadOnlySpan();

            if (_currentSpan.IsEmpty)
            {
                return false;
            }

            return true;
        }
    }

    private sealed class QueueBlock
    {
        private readonly T[] _items;
        private readonly int _capacity;
        private int _index;

        public QueueBlock? Next;

        public bool IsFull
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _index >= _capacity;
        }

        public QueueBlock(int capacity)
        {
            _items = new T[capacity];
            _capacity = capacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnqueue(in T item)
        {
            int index = Interlocked.Increment(ref _index) - 1;

            if (index < _capacity)
            {
                ref T slot = ref Unsafe.Add(
                    ref MemoryMarshal.GetArrayDataReference(_items),
                    index);
                slot = item;

                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan()
        {
            int currentIndex = _index;
            int count = Math.Min(currentIndex, _capacity);

            if (count == 0)
            {
                return ReadOnlySpan<T>.Empty;
            }

            return _items.AsSpan(0, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            _index = 0;
        }
    }
}
