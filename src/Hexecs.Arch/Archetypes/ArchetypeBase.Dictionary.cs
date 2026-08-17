using Hexecs.Arch.Utils;

namespace Hexecs.Arch.Archetypes;

internal abstract partial class ArchetypeBase<TEntry>
{
    private uint[] _sparse = new uint[capacity];
    private uint[] _dense = new uint[capacity];
    private TEntry[] _values = new TEntry[capacity];
    private int _count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool ContainsEntry(uint ownerId)
    {
        var sparse = _sparse;
        if (ownerId < (uint)sparse.Length)
        {
            var denseIndexPlusOne = sparse[ownerId];
            return denseIndexPlusOne != 0 && _dense[denseIndexPlusOne - 1] == ownerId;
        }

        return false;
    }

    private void EnsureCapacity(uint ownerId)
    {
        // Проверка емкости плотных массивов (количество элементов)
        if (_count >= _dense.Length)
        {
            var newSize = _dense.Length * 2;
            Array.Resize(ref _dense, newSize);
            Array.Resize(ref _values, newSize);
        }

        // Проверка емкости разреженного массива (максимальный ID)
        if (ownerId >= (uint)_sparse.Length)
        {
            var newSize = Math.Max((uint)_sparse.Length * 2, ownerId + 1);
            Array.Resize(ref _sparse, (int)newSize);
        }
    }

    private bool RemoveEntry(uint ownerId, out TEntry value)
    {
        var sparse = _sparse;
        if (ownerId < (uint)sparse.Length)
        {
            var slot = sparse[ownerId];
            if (slot != 0)
            {
                var denseIndex = (int)slot - 1;
                if (_dense[denseIndex] == ownerId)
                {
                    ref var componentRef = ref _values[denseIndex];
                    value = componentRef;

                    // remove events here

                    var lastIndex = _count - 1;
                    if (denseIndex != lastIndex)
                    {
                        var lastKey = _dense[lastIndex];
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
    protected bool TryAddEntry(uint ownerId, out Ref<TEntry> reference)
    {
        // Fast Path: если ID влезает в массив и есть место в dense
        if (ownerId < (uint)_sparse.Length && (uint)_count < (uint)_dense.Length)
        {
            ref var slot = ref _sparse[ownerId];
            if (slot == 0)
            {
                var idx = (uint)_count;
                slot = idx + 1;
                _dense[idx] = ownerId;

                reference = new Ref<TEntry>(ref _values[idx]);
                _count++;
                return true;
            }

            if (_dense[slot - 1] == ownerId)
            {
                reference = Ref<TEntry>.Empty;
                return false;
            }
        }

        return TryAddEntrySlow(ownerId, out reference);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool TryAddEntrySlow(uint ownerId, out Ref<TEntry> reference)
    {
        EnsureCapacity(ownerId);

        ref var denseIndexPlusOne = ref _sparse[ownerId];
        if (denseIndexPlusOne != 0)
        {
            if (_dense[denseIndexPlusOne - 1] == ownerId)
            {
                reference = Ref<TEntry>.Empty;
                return false;
            }
        }

        var denseIndex = (uint)_count;
        denseIndexPlusOne = denseIndex + 1;
        _dense[denseIndex] = ownerId;

        reference = new Ref<TEntry>(ref _values[denseIndex]);
        _count++;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref TEntry TryGetEntryRef(uint actorId)
    {
        var sparse = _sparse;
        if (actorId < (uint)sparse.Length)
        {
            var denseIndexPlusOne = sparse[actorId];
            if (denseIndexPlusOne != 0)
            {
                var index = (int)denseIndexPlusOne - 1;
                if (_dense[index] == actorId)
                {
                    return ref _values[index];
                }
            }
        }

        return ref Unsafe.NullRef<TEntry>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool UpsertEntry(uint ownerId, out bool exists, out Ref<TEntry> reference)
    {
        if (ownerId < (uint)_sparse.Length)
        {
            ref var slot = ref _sparse[ownerId];
            if (slot != 0)
            {
                var denseIndex = (int)slot - 1;
                if (_dense[denseIndex] == ownerId)
                {
                    exists = true;
                    reference = new Ref<TEntry>(ref _values[denseIndex]);
                    return true;
                }
            }

            if ((uint)_count < (uint)_dense.Length)
            {
                var idx = _count;
                slot = (uint)idx + 1;
                _dense[idx] = ownerId;
                _count = idx + 1;
                exists = false;
                reference = new Ref<TEntry>(ref _values[idx]);
                return true;
            }
        }

        exists = false;
        return TryAddEntrySlow(ownerId, out reference);
    }
}