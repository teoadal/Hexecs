namespace Hexecs.Actors.Components;

internal sealed partial class ActorComponentPool<T>
{
    private uint[] _sparse;
    private uint[] _dense;
    private T[] _values;
    private int _count;

    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool ContainsEntry(uint ownerId)
    {
        uint[] sparse = _sparse;

        if (ownerId < (uint)sparse.Length)
        {
            uint denseIndexPlusOne = sparse[ownerId];

            return denseIndexPlusOne != 0 && _dense[denseIndexPlusOne - 1] == ownerId;
        }

        return false;
    }

    private void EnsureCapacity(uint ownerId)
    {
        // Проверка емкости плотных массивов (количество элементов)
        if (_count >= _dense.Length)
        {
            int newSize = _dense.Length * 2;
            Array.Resize(ref _dense, newSize);
            Array.Resize(ref _values, newSize);
        }

        // Проверка емкости разреженного массива (максимальный ID)
        if (ownerId >= (uint)_sparse.Length)
        {
            uint newSize = Math.Max((uint)_sparse.Length * 2, ownerId + 1);
            Array.Resize(ref _sparse, (int)newSize);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref T GetEntryRef(uint actorId)
    {
        uint[] sparse = _sparse;

        if (actorId < (uint)sparse.Length)
        {
            uint denseIndexPlusOne = sparse[actorId];

            if (denseIndexPlusOne != 0)
            {
                int index = (int)denseIndexPlusOne - 1;

                if (_dense[index] == actorId)
                {
                    return ref _values[index];
                }
            }
        }

        return ref Unsafe.NullRef<T>();
    }

    private bool RemoveEntry(uint ownerId, out T value)
    {
        uint[] sparse = _sparse;

        if (ownerId < (uint)sparse.Length)
        {
            uint slot = sparse[ownerId];

            if (slot != 0)
            {
                int denseIndex = (int)slot - 1;

                if (_dense[denseIndex] == ownerId)
                {
                    ref T componentRef = ref _values[denseIndex];
                    value = componentRef;

                    var actorId = new ActorId(_dense[denseIndex]);
                    Removing?.Invoke(actorId);
                    ComponentRemoving?.Invoke(actorId, ref componentRef);
                    _disposeHandler?.Invoke(ref componentRef);

                    int lastIndex = _count - 1;

                    if (denseIndex != lastIndex)
                    {
                        uint lastKey = _dense[lastIndex];
                        _dense[denseIndex] = lastKey;
                        _values[denseIndex] = _values[lastIndex];

                        // Обновляем указатель в sparse для переехавшего элемента
                        _sparse[lastKey] = slot;
                    }

                    _sparse[ownerId] = 0;
                    _count = lastIndex;

                    return true;
                }
            }
        }

        value = default;

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private AddResult TryAddEntry(uint ownerId)
    {
        // Fast Path: если ID влезает в массив и есть место в dense
        if (ownerId < (uint)_sparse.Length && (uint)_count < (uint)_dense.Length)
        {
            ref uint slot = ref _sparse[ownerId];

            if (slot == 0)
            {
                var idx = (uint)_count;
                slot = idx + 1;
                _dense[idx] = ownerId;

                ref T internalRef = ref _values[idx];
                AddResult result = AddResult.Success(_count, ref internalRef);
                _count++;

                return result;
            }

            if (_dense[slot - 1] == ownerId)
            {
                return AddResult.Failure();
            }
        }

        return TryAddEntrySlow(ownerId);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private AddResult TryAddEntrySlow(uint ownerId)
    {
        EnsureCapacity(ownerId);

        ref uint denseIndexPlusOne = ref _sparse[ownerId];

        if (denseIndexPlusOne != 0)
        {
            if (_dense[denseIndexPlusOne - 1] == ownerId)
            {
                return AddResult.Failure();
            }
        }

        var denseIndex = (uint)_count;
        denseIndexPlusOne = denseIndex + 1;
        _dense[denseIndex] = ownerId;

        ref T internalRef = ref _values[denseIndex];
        AddResult result = AddResult.Success(_count, ref internalRef);
        _count++;

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private AddResult UpsertEntry(uint ownerId, out bool exists)
    {
        if (ownerId < (uint)_sparse.Length)
        {
            ref uint slot = ref _sparse[ownerId];

            if (slot != 0)
            {
                int denseIndex = (int)slot - 1;

                if (_dense[denseIndex] == ownerId)
                {
                    exists = true;

                    return AddResult.Success(denseIndex, ref _values[denseIndex]);
                }
            }

            if ((uint)_count < (uint)_dense.Length)
            {
                int idx = _count;
                slot = (uint)idx + 1;
                _dense[idx] = ownerId;
                _count = idx + 1;
                exists = false;

                return AddResult.Success(idx, ref _values[idx]);
            }
        }

        exists = false;

        return TryAddEntrySlow(ownerId);
    }
}
