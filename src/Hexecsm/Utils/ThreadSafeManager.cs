namespace Hexecsm.Utils;

// ReSharper disable InvertIf
internal abstract class ThreadSafeManager<TContract>(int initialCapacity)
    where TContract : class
{
    private TContract?[] _items = new TContract?[initialCapacity];
    private readonly Lock _lock = new Lock();

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<TContract?> GetAll()
    {
        return Volatile.Read(ref _items);
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected TContract GetItemUnsafe(uint index)
    {
        return Volatile.Read(ref _items)[index]!;
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected TContract? GetItem(uint index)
    {
        TContract?[] pools = Volatile.Read(ref _items);

        if (index < (uint)pools.Length)
        {
            return Volatile.Read(ref pools[index]);
        }

        return null;
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected TContract GetOrAddItem<TArg>(uint index, Func<TArg, TContract> factory, TArg arg)
        where TArg : class
    {
        TContract?[] pools = Volatile.Read(ref _items);

        if (index < (uint)pools.Length)
        {
            TContract? existsPool = Volatile.Read(ref pools[index]);

            if (existsPool != null)
            {
                return existsPool;
            }
        }

        return CreateItem(index, factory, arg);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private TContract CreateItem<TArg>(uint index, Func<TArg, TContract> factory, TArg arg)
        where TArg : class
    {
        using (_lock.EnterScope())
        {
            TContract?[] pools = _items;

            if (index < (uint)pools.Length)
            {
                TContract? existsPool = pools[index];

                if (existsPool != null)
                {
                    return existsPool;
                }
            }

            int requiredCapacity = (int)index + 1;
            TContract?[] localStorage = _items;

            TContract newPool = factory(arg);

            if (requiredCapacity > localStorage.Length)
            {
                // Сценарий 1: Массив РАСШИРЯЕТСЯ
                int newCapacity = Math.Max(localStorage.Length * 2, requiredCapacity);
                Array.Resize(ref localStorage, newCapacity);

                localStorage[index] = newPool;

                // Барьер гарантирует, что новый массив полностью заполнен в памяти
                Thread.MemoryBarrier();

                // Атомарно публикуем новую ссылку. Читающие потоки мгновенно
                // увидят и новую длину Span, и новые данные.
                Volatile.Write(ref _items, localStorage);
            }
            else
            {
                // Сценарий 2: Массив ТОТ ЖЕ. Пустая ячейка просто заполняется.
                Volatile.Write(ref localStorage[index], newPool);
            }

            return newPool;
        }
    }
}
