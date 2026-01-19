namespace Hexecs.Actors;

[SuppressMessage("ReSharper", "InvertIf")]
public sealed partial class ActorFilter<T1>
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
    private int _count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddEntry(uint actorId)
    {
        if (TryAddEntry(actorId)) return;
        ActorError.AlreadyExists(actorId);
    }

    private void ClearEntries()
    {
        var dense = _dense;
        var sparsePages = _sparsePages;

        // Очищаем только занятые ячейки в разреженных страницах за O(Count)
        for (var i = 0; i < _count; i++)
        {
            var key = dense[i];
            var pageIndex = (int)(key >> PageBits);
            sparsePages[pageIndex]![key & PageMask] = 0;
        }

        _count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool ContainsEntry(uint actorId)
    {
        var pageIndex = (int)(actorId >> PageBits);
        var pages = _sparsePages;

        if ((uint)pageIndex < (uint)pages.Length)
        {
            var page = pages[pageIndex];
            if (page != null)
            {
                var denseIndexPlusOne = page[actorId & PageMask];
                return denseIndexPlusOne != 0 && _dense[denseIndexPlusOne - 1] == actorId;
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

    private bool RemoveEntry(uint actorId)
    {
        var pageIndex = (int)(actorId >> PageBits);
        if ((uint)pageIndex >= (uint)_sparsePages.Length) return false;

        var page = _sparsePages[pageIndex];
        if (page == null) return false;

        var offset = (int)(actorId & PageMask);
        var denseIndexPlusOne = page[offset];
        if (denseIndexPlusOne == 0) return false;

        var denseIndex = (int)denseIndexPlusOne - 1;
        if (_dense[denseIndex] != actorId) return false;

        var lastIndex = _count - 1;
        if (denseIndex != lastIndex)
        {
            var lastKey = _dense[lastIndex];
            _dense[denseIndex] = lastKey;

            var lastKeyPageIndex = (int)(lastKey >> PageBits);
            _sparsePages[lastKeyPageIndex]![lastKey & PageMask] = (uint)denseIndex + 1;
        }

        page[offset] = 0;
        _count = lastIndex;

        Removed?.Invoke(actorId);

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryAddEntry(uint actorId)
    {
        var pageIndex = (int)(actorId >> PageBits);
        var pages = _sparsePages;

        if ((uint)pageIndex < (uint)pages.Length)
        {
            var page = pages[pageIndex];
            if (page != null && (uint)_count < (uint)_dense.Length)
            {
                ref var slot = ref page[actorId & PageMask];
                if (slot == 0)
                {
                    var idx = (uint)_count;
                    slot = idx + 1;
                    _dense[idx] = actorId;
                    _count++;

                    Added?.Invoke(actorId);

                    return true;
                }

                if (_dense[slot - 1] == actorId) return false;
            }
        }

        return TryAddEntrySlow(actorId);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool TryAddEntrySlow(uint actorId)
    {
        EnsureDenseCapacity();
        var pageIndex = (int)(actorId >> PageBits);
        EnsurePageArraySize(pageIndex);

        ref var page = ref _sparsePages[pageIndex];
        if (page == null)
        {
            page = ArrayUtils.Create<uint>(PageSize);
            Array.Clear(page, 0, page.Length);
        }

        ref var denseIndexPlusOne = ref page[actorId & PageMask];
        if (denseIndexPlusOne != 0)
        {
            if (_dense[denseIndexPlusOne - 1] == actorId) return false;
        }

        var denseIndex = (uint)_count;
        denseIndexPlusOne = denseIndex + 1;
        _dense[denseIndex] = actorId;
        _count++;

        Added?.Invoke(actorId);

        return true;
    }
}