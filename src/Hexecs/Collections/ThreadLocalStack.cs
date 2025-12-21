namespace Hexecs.Collections;

/// <summary>
/// Потокобезопасный стек с использованием локальных буферов потоков.
/// Оптимизирован для Zero-Allocation и минимальной конкуренции.
/// </summary>
internal sealed class ThreadLocalStack<T> : IDisposable where T : struct
{
    private const int LocalCapacity = 128;
    private const int BatchSize = 64;

    private readonly ThreadLocal<LocalBuffer> _localStack;
    private readonly Stack<T> _globalStack;
#if NET9_0_OR_GREATER
    private readonly Lock _globalLock = new();
#else
    private readonly object _globalLock = new();
#endif
    private bool _isDisposed;

    public ThreadLocalStack(int globalCapacity = 1024)
    {
        _globalStack = new Stack<T>(globalCapacity);
        _localStack = new ThreadLocal<LocalBuffer>(static () => new LocalBuffer(), trackAllValues: true);
    }

    public void Clear()
    {
#if NET9_0_OR_GREATER
        using (_globalLock.EnterScope())
#else
        lock (_globalLock)
#endif
        {
            _globalStack.Clear();
        }

        foreach (var local in _localStack.Values)
        {
            local.Count = 0;
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        // Сначала ставим флаг, чтобы другие потоки перестали работать со стеком
        _isDisposed = true;

        // Захватываем глобальный лок один раз на весь процесс очистки
        lock (_globalLock)
        {
            // Проверяем буферы только если ThreadLocal еще жив
            try
            {
                foreach (var local in _localStack.Values)
                {
                    while (local.Count > 0)
                    {
                        _globalStack.Push(local.Data[--local.Count]);
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                /* Игнорируем, если уже очищено */
            }

            _localStack.Dispose();
            _globalStack.Clear();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(T item)
    {
        var local = _localStack.Value!;
        if (local.Count < LocalCapacity)
        {
            local.Data[local.Count++] = item;
            return;
        }

        PushToGlobal(local);
        local.Data[local.Count++] = item;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPop(out T result)
    {
        var local = _localStack.Value!;
        if (local.Count > 0)
        {
            result = local.Data[--local.Count];
            return true;
        }

        if (TryPopFromGlobal(local) && local.Count > 0)
        {
            result = local.Data[--local.Count];
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Сбрасывает содержимое локального буфера текущего потока в глобальный стек.
    /// Полезно вызывать при завершении работы потока или системы.
    /// </summary>
    public void Flush()
    {
        if (_localStack.IsValueCreated)
        {
            var local = _localStack.Value!;
            if (local.Count > 0)
            {
#if NET9_0_OR_GREATER
                using (_globalLock.EnterScope())
#else
                lock (_globalLock)
#endif
                {
                    while (local.Count > 0)
                    {
                        _globalStack.Push(local.Data[--local.Count]);
                    }
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void PushToGlobal(LocalBuffer local)
    {
#if NET9_0_OR_GREATER
        using (_globalLock.EnterScope())
#else
        lock (_globalLock)
#endif
        {
            for (var i = 0; i < BatchSize; i++)
            {
                _globalStack.Push(local.Data[--local.Count]);
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool TryPopFromGlobal(LocalBuffer local)
    {
#if NET9_0_OR_GREATER
        using (_globalLock.EnterScope())
#else
        lock (_globalLock)
#endif
        {
            if (_globalStack.Count == 0) return false;

            var toFetch = Math.Min(_globalStack.Count, BatchSize);
            // Проверяем свободное место (на всякий случай)
            toFetch = Math.Min(toFetch, LocalCapacity - local.Count);

            for (var i = 0; i < toFetch; i++)
            {
                local.Data[local.Count++] = _globalStack.Pop();
            }

            return toFetch > 0;
        }
    }

    private sealed class LocalBuffer
    {
        public readonly T[] Data = ArrayUtils.Create<T>(LocalCapacity);
        public int Count;
    }
}