using System.Runtime.InteropServices;

namespace Hexecs.Benchmarks.Collections;

public sealed class MpscRingBuffer<T> where T : struct
{
    private readonly long[] _sequences;
    private readonly T[] _items;
    private readonly int _mask;

    private PaddedHeadTail _indices;

    public MpscRingBuffer(int powerOfTwoCapacity)
    {
        if ((powerOfTwoCapacity & (powerOfTwoCapacity - 1)) != 0)
        {
            throw new ArgumentException("Размер буфера должен быть степенью двойки.");
        }

        _items = new T[powerOfTwoCapacity];
        _sequences = new long[powerOfTwoCapacity];
        _mask = powerOfTwoCapacity - 1;

        for (var i = 0; i < _sequences.Length; i++)
        {
            _sequences[i] = i;
        }
    }

    // Множество писателей (Multi-Producer)
    public bool TryEnqueue(in T item)
    {
        while (true)
        {
            long currentTail = Volatile.Read(ref _indices.Tail);
            var index = (int)(currentTail & _mask);

            long cellSequence = Volatile.Read(ref _sequences[index]);
            long diff = cellSequence - currentTail;

            if (diff == 0)
            {
                // Пытаемся занять слот атомарно
                if (Interlocked.CompareExchange(ref _indices.Tail, currentTail + 1, currentTail) == currentTail)
                {
                    _items[index] = item;

                    // Барьер памяти: данные _items обязаны быть записаны ДО обновления статуса в _sequences
                    Volatile.Write(ref _sequences[index], currentTail + 1);

                    return true;
                }
            }
            else if (diff < 0)
            {
                // Буфер переполнен
                return false;
            }

            // Надвигается гонка или слот занят другим писателем — уходим на короткий Spin
            Thread.SpinWait(1);
        }
    }

    // Один читатель (Single-Consumer)
    public bool TryDequeue(out T item)
    {
        long currentHead = _indices.Head;
        var index = (int)(currentHead & _mask);

        long cellSequence = Volatile.Read(ref _sequences[index]);
        long diff = cellSequence - (currentHead + 1);

        if (diff == 0)
        {
            item = _items[index];

            // Если T содержит ссылки (хоть мы и ограничили struct, внутри могут быть Nullable или Ref-поля)
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                _items[index] = default;
            }

            // Разрешаем писателям снова использовать этот слот на следующем круге
            Volatile.Write(ref _sequences[index], currentHead + _sequences.Length);

            // Сдвигаем голову без Interlocked (читатель-то один)
            Volatile.Write(ref _indices.Head, currentHead + 1);

            return true;
        }

        item = default;

        return false;
    }
}

// Выносим индексы в отдельную структуру без Generic, чтобы работал Explicit Layout
[StructLayout(LayoutKind.Explicit, Size = 128)]
internal struct PaddedHeadTail
{
    [FieldOffset(0)]
    public long Head;

    [FieldOffset(64)]
    public long Tail; // Разнос на 64 байта (размер кэш-линии)
}

public sealed class MpscBatchingRingBuffer<T> where T : struct
{
    private readonly long[] _sequences;
    private readonly T[] _items;
    private readonly int _mask;

    // Структура индексов с ручным Padding для исключения False Sharing
    private struct PaddedIndices
    {
        public long P1, P2, P3, P4, P5, P6, P7, P8;
        public long Head;
        public long P9, P10, P11, P12, P13, P14, P15, P16;
        public long Tail;
    }

    private PaddedIndices _indices;

    public MpscBatchingRingBuffer(int powerOfTwoCapacity)
    {
        if ((powerOfTwoCapacity & (powerOfTwoCapacity - 1)) != 0)
        {
            throw new ArgumentException("Размер должен быть степенью двойки.");
        }

        _items = new T[powerOfTwoCapacity];
        _sequences = new long[powerOfTwoCapacity];
        _mask = powerOfTwoCapacity - 1;

        for (var i = 0; i < _sequences.Length; i++)
        {
            _sequences[i] = i;
        }
    }

    // Lock-Free Enqueue для множества писателей
    public bool TryEnqueue(in T item)
    {
        while (true)
        {
            long currentTail = Volatile.Read(ref _indices.Tail);
            var index = (int)(currentTail & _mask);

            long cellSequence = Volatile.Read(ref _sequences[index]);
            long diff = cellSequence - currentTail;

            if (diff == 0)
            {
                if (Interlocked.CompareExchange(ref _indices.Tail, currentTail + 1, currentTail) == currentTail)
                {
                    _items[index] = item;

                    // Release barrier: данные записаны до обновления Sequence
                    Volatile.Write(ref _sequences[index], currentTail + 1);

                    return true;
                }
            }
            else if (diff < 0)
            {
                return false; // Буфер полон
            }

            Thread.SpinWait(1);
        }
    }

    /// <summary>
    /// Возвращает ReadOnlySpan на пачку гарантированно записанных данных.
    /// Вызывается строго в ОДНОМ потоке-читателе.
    /// </summary>
    public Span<T> GetReadableSpan(out long batchHeadStart, out int processedCount)
    {
        long currentHead = _indices.Head;
        var index = (int)(currentHead & _mask);

        // Оптимизация: Считываем хвост один раз. Мы точно знаем, что писатели не могли
        // уйти дальше этого значения на момент вызова метода.
        long capturedTail = Volatile.Read(ref _indices.Tail);

        if (currentHead >= capturedTail)
        {
            batchHeadStart = currentHead;
            processedCount = 0;

            return Span<T>.Empty;
        }

        // Вычисляем, сколько элементов теоретически доступно до конца массива (без заворота)
        var maxAvailableInTrend = (int)Math.Min(capturedTail - currentHead, _items.Length - index);
        var count = 0;

        while (count < maxAvailableInTrend)
        {
            long nextHead = currentHead + count;
            var nextIdx = (int)(nextHead & _mask);
            long nextSeq = Volatile.Read(ref _sequences[nextIdx]);

            // Проверяем флаг готовности ячейки
            if (nextSeq - (nextHead + 1) == 0)
            {
                count++;
            }
            else
            {
                // Наткнулись на ячейку, где Interlocked у писателя уже прошел, но данные еще не скопированы
                break;
            }
        }

        batchHeadStart = currentHead;
        processedCount = count;

        if (count == 0)
        {
            return Span<T>.Empty;
        }

        // Абсолютно безопасный Span: писатели гарантированно не полезут в эти индексы,
        // пока мы не вызовем AdvanceReader и не обновим _sequences
        return new Span<T>(_items, index, count);
    }

    /// <summary>
    /// Освобождает обработанные ячейки и возвращает их писателям.
    /// </summary>
    public void AdvanceReader(long headStart, int count)
    {
        if (count <= 0)
        {
            return;
        }

        bool hasReferences = RuntimeHelpers.IsReferenceOrContainsReferences<T>();

        for (var i = 0; i < count; i++)
        {
            long seqToRelease = headStart + i;
            var idx = (int)(seqToRelease & _mask);

            // КРИТИЧЕСКОЕ ИСПРАВЛЕНИЕ: сначала очищаем память, если структура содержит ссылки...
            if (hasReferences)
            {
                _items[idx] = default;
            }

            // ...и только ПОТОМ публикуем sequence, разрешая писателям занять этот слот
            Volatile.Write(ref _sequences[idx], seqToRelease + _items.Length);
        }

        // Обновляем глобальную голову один раз на всю пачку
        Volatile.Write(ref _indices.Head, headStart + count);
    }
}
