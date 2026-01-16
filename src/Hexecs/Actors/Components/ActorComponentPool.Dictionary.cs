namespace Hexecs.Actors.Components;

internal sealed partial class ActorComponentPool<T>
{
    private const int PageBits = 12;
    private const int PageSize = 1 << PageBits; // 4096
    private const int PageMask = PageSize - 1;

    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _count;
    }

    private uint[]?[] _sparsePages;
    private uint[] _dense;
    private T[] _values;
    private int _count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool ContainsEntry(uint ownerId)
    {
        var pageIndex = (int)(ownerId >> PageBits);
        var pages = _sparsePages;

        if ((uint)pageIndex < (uint)pages.Length)
        {
            var page = pages[pageIndex];
            if (page != null)
            {
                var denseIndexPlusOne = page[ownerId & PageMask];
                return denseIndexPlusOne != 0 && _dense[denseIndexPlusOne - 1] == ownerId;
            }
        }

        return false;
    }

    private void EnsureDenseCapacity()
    {
        if (_count >= _dense.Length)
        {
            var newSize = _dense.Length * 2;
            Array.Resize(ref _dense, newSize);
            Array.Resize(ref _values, newSize);
        }
    }

    private void EnsurePageArraySize(int pageIndex)
    {
        if (pageIndex >= _sparsePages.Length)
        {
            var newSize = Math.Max(_sparsePages.Length * 2, pageIndex + 1);
            Array.Resize(ref _sparsePages, newSize);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref T GetEntryRef(uint actorId)
    {
        var pageIndex = (int)(actorId >> PageBits);
        if ((uint)pageIndex < (uint)_sparsePages.Length)
        {
            var page = _sparsePages[pageIndex];
            if (page != null)
            {
                var denseIndexPlusOne = page[actorId & PageMask];
                if (denseIndexPlusOne != 0)
                {
                    var index = (int)denseIndexPlusOne - 1;
                    if (_dense[index] == actorId)
                    {
                        return ref _values[index];
                    }
                }
            }
        }

        return ref Unsafe.NullRef<T>();
    }

    private bool RemoveEntry(uint ownerId, out T value)
    {
        var pageIndex = (int)(ownerId >> PageBits);
        var pages = _sparsePages;

        if ((uint)pageIndex < (uint)pages.Length)
        {
            var page = pages[pageIndex];
            if (page != null)
            {
                var offset = (int)(ownerId & PageMask);
                var slot = page[offset];
                if (slot != 0)
                {
                    var denseIndex = (int)slot - 1;
                    if (_dense[denseIndex] == ownerId)
                    {
                        ref var componentRef = ref _values[denseIndex];
                        value = componentRef;

                        Removing?.Invoke(ownerId);
                        ComponentRemoving?.Invoke(ownerId, ref componentRef);
                        _disposeHandler?.Invoke(ref componentRef);

                        var lastIndex = _count - 1;
                        if (denseIndex != lastIndex)
                        {
                            var lastKey = _dense[lastIndex];
                            _dense[denseIndex] = lastKey;
                            _values[denseIndex] = _values[lastIndex];

                            var lastKeyPageIndex = (int)(lastKey >> PageBits);
                            pages[lastKeyPageIndex]![lastKey & PageMask] = slot;
                        }

                        page[offset] = 0;
                        _count = lastIndex;
                        return true;
                    }
                }
            }
        }

        value = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private AddResult TryAddEntry(uint ownerId)
    {
        var pageIndex = (int)(ownerId >> PageBits);
        var pages = _sparsePages;

        // Максимально компактная проверка на готовность страницы и места
        if ((uint)pageIndex < (uint)pages.Length)
        {
            var page = pages[pageIndex];
            if (page != null && (uint)_count < (uint)_dense.Length)
            {
                ref var slot = ref page[ownerId & PageMask];
                if (slot == 0) // Чистая вставка (самый частый случай в ECS)
                {
                    var idx = (uint)_count;
                    slot = idx + 1;
                    _dense[idx] = ownerId;

                    ref var internalRef = ref _values[idx];
                    var result = AddResult.Success(_count, ref internalRef);

                    _count++;

                    return result;
                }

                // Если не 0, проверяем на дубликат (чуть медленнее)
                if (_dense[slot - 1] == ownerId) return AddResult.Failure();
            }
        }

        return TryAddEntrySlow(ownerId);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private AddResult TryAddEntrySlow(uint ownerId)
    {
        EnsureDenseCapacity();
        var pageIndex = (int)(ownerId >> PageBits);
        EnsurePageArraySize(pageIndex);

        ref var page = ref _sparsePages[pageIndex];
        if (page == null)
        {
            page = ArrayUtils.Create<uint>(PageSize);
            Array.Clear(page, 0, page.Length);
        }

        ref var denseIndexPlusOne = ref page[ownerId & PageMask];
        if (denseIndexPlusOne != 0)
        {
            if (_dense[denseIndexPlusOne - 1] == ownerId) return AddResult.Failure();
        }

        var denseIndex = (uint)_count;
        denseIndexPlusOne = denseIndex + 1;
        _dense[denseIndex] = ownerId;

        ref var internalRef = ref _values[denseIndex];
        var result = AddResult.Success(_count, ref internalRef);

        _count++;

        return result;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int TryGetEntryIndex(uint ownerId)
    {
        var pageIndex = (int)(ownerId >> PageBits);
        var pages = _sparsePages;
        
        if ((uint)pageIndex < (uint)pages.Length)
        {
            var page = pages[pageIndex];
            if (page != null)
            {
                var slot = page[ownerId & PageMask];
                if (slot != 0)
                {
                    var denseIndex = (int)slot - 1;
                    if (_dense[denseIndex] == ownerId)
                    {
                        return denseIndex;
                    }
                }
            }
        }

        return -1;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private AddResult UpsertEntry(uint ownerId, out bool exists)
    {
        var pageIndex = (int)(ownerId >> PageBits);
        var pages = _sparsePages;

        // 1. Пытаемся найти существующий (Fast Path)
        if ((uint)pageIndex < (uint)pages.Length)
        {
            var page = pages[pageIndex];
            if (page != null)
            {
                ref var slot = ref page[ownerId & PageMask];
                if (slot != 0)
                {
                    var denseIndex = (int)slot - 1;
                    if (_dense[denseIndex] == ownerId)
                    {
                        exists = true;
                        return AddResult.Success(denseIndex, ref _values[denseIndex]);
                    }
                }
                    
                // 2. Если страница есть, но ключа нет, и есть место - добавляем сразу (Fast Add)
                if ((uint)_count < (uint)_dense.Length)
                {
                    var idx = _count;
                    slot = (uint)idx + 1;
                    _dense[idx] = ownerId;
                    _count = idx + 1;
                    exists = false;
                    return AddResult.Success(idx, ref _values[idx]);
                }
            }
        }

        // 3. Если всё сложно (нужен ресайз или новая страница) - идем в Slow Path
        exists = false;
        return TryAddEntrySlow(ownerId);
    }
}