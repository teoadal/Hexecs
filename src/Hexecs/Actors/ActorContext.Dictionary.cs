using Hexecs.Actors.Relations;

namespace Hexecs.Actors;

[SuppressMessage("ReSharper", "InvertIf")]
public sealed partial class ActorContext
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
    private Entry[] _values;
    private int _count;

    private ref Entry AddEntry(uint actorId)
    {
        ref var entry = ref TryAddEntry(actorId);
        if (!Unsafe.IsNullRef(ref entry))
        {
            Created?.Invoke(actorId);
            return ref entry;
        }

        ActorError.AlreadyExists(actorId); // выбрасывает ошибку
        return ref Unsafe.NullRef<Entry>();
    }

    private void ClearEntry(uint actorId, ref Entry entry)
    {
        ref var relationsComponent = ref TryGetComponentRef<ActorRelationComponent>(actorId);
        if (!Unsafe.IsNullRef(ref relationsComponent))
        {
            foreach (var relationId in relationsComponent)
            {
                var relationPool = _relationPools[relationId];
                relationPool?.Remove(actorId);
            }
        }

        foreach (var componentId in entry)
        {
            var componentPool = _componentPools[componentId];
            componentPool?.Remove(actorId);
        }

        entry.Dispose();
    }

    private void ClearEntries()
    {
        var dense = _dense;
        var values = _values;
        var sparsePages = _sparsePages;

        for (var i = 0; i < _count; i++)
        {
            var key = dense[i];

            ref var entry = ref values[i];
            entry.Dispose();

            var pageIndex = (int)(key >> PageBits);
            sparsePages[pageIndex]![key & PageMask] = 0;
        }

        _count = 0;
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
    private ref Entry GetEntryRef(uint actorId)
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

        return ref Unsafe.NullRef<Entry>();
    }

    private ref Entry GetEntryRefExact(uint key)
    {
        ref var entry = ref GetEntryRef(key);
        if (!Unsafe.IsNullRef(ref entry))
        {
            return ref entry;
        }

        ActorError.NotFound(key); // exception
        return ref Unsafe.NullRef<Entry>();
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

        // 1. Уведомляем системы
        Destroying?.Invoke(actorId);

        // 2. Получаем ссылку ОДИН раз и работаем через неё
        ref var entryToRemove = ref _values[denseIndex];
        ClearEntry(actorId, ref entryToRemove);

        var lastIndex = _count - 1;
        if (denseIndex != lastIndex)
        {
            var lastKey = _dense[lastIndex];

            // Переносим ключ
            _dense[denseIndex] = lastKey;

            // Копируем данные из последней ячейки в текущую (удаляемую) ссылку
            // Это заменяет _values[denseIndex] = _values[lastIndex]
            entryToRemove = _values[lastIndex];

            // Обновляем индекс перемещенного ключа в sparse-страницах
            var lastKeyPageIndex = (int)(lastKey >> PageBits);
            _sparsePages[lastKeyPageIndex]![lastKey & PageMask] = (uint)denseIndex + 1;
        }

        // 3. Зачищаем хвост
        page[offset] = 0;
        _count = lastIndex;

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref Entry TryAddEntry(uint actorId)
    {
        var pageIndex = (int)(actorId >> PageBits);
        var pages = _sparsePages;

        // Максимально компактная проверка на готовность страницы и места
        if ((uint)pageIndex < (uint)pages.Length)
        {
            var page = pages[pageIndex];
            if (page != null && (uint)_count < (uint)_dense.Length)
            {
                ref var slot = ref page[actorId & PageMask];
                if (slot == 0) // Чистая вставка (самый частый случай в ECS)
                {
                    var idx = (uint)_count;
                    slot = idx + 1;
                    _dense[idx] = actorId;
                    _count++;

                    return ref _values[idx];
                }

                // Если не 0, проверяем на дубликат (чуть медленнее)
                if (_dense[slot - 1] == actorId)
                {
                    return ref Unsafe.NullRef<Entry>();
                }
            }
        }

        return ref TryAddEntrySlow(actorId);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private ref Entry TryAddEntrySlow(uint actorId)
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
            if (_dense[denseIndexPlusOne - 1] == actorId)
            {
                return ref Unsafe.NullRef<Entry>();
            }
        }

        var denseIndex = (uint)_count;
        denseIndexPlusOne = denseIndex + 1;
        _dense[denseIndex] = actorId;
        _count++;

        return ref _values[denseIndex];
    }
}