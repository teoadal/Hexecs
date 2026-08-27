namespace Hexecs.Actors;

[SuppressMessage("ReSharper", "InvertIf")]
public sealed partial class ActorFilter<T1>
{
    public ReadOnlySpan<uint> Keys
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _dense.AsSpan(0, _count);
    }

    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _count;
    }

    private uint[] _sparse;
    private uint[] _dense;
    private int _count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddEntry(uint actorId)
    {
        if (TryAddEntry(actorId))
        {
            return;
        }

        ActorError.AlreadyExists(actorId);
    }

    private void ClearEntries()
    {
        uint[] dense = _dense;
        uint[] sparse = _sparse;

        for (var i = 0; i < _count; i++)
        {
            sparse[dense[i]] = 0;
        }

        _count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool ContainsEntry(uint actorId)
    {
        uint[] sparse = _sparse;
        if (actorId < (uint)sparse.Length)
        {
            uint denseIndexPlusOne = sparse[actorId];
            return denseIndexPlusOne != 0 && _dense[denseIndexPlusOne - 1] == actorId;
        }

        return false;
    }

    private void EnsureCapacity(uint actorId)
    {
        if (_count >= _dense.Length)
        {
            int newSize = _dense.Length * 2;
            Array.Resize(ref _dense, newSize);
        }

        if (actorId >= (uint)_sparse.Length)
        {
            uint newSize = Math.Max((uint)_sparse.Length * 2, actorId + 1);
            Array.Resize(ref _sparse, (int)newSize);
        }
    }

    private bool RemoveEntry(uint actorId)
    {
        uint[] sparse = _sparse;
        if (actorId < (uint)sparse.Length)
        {
            uint slot = sparse[actorId];
            if (slot != 0)
            {
                int denseIndex = (int)slot - 1;
                if (_dense[denseIndex] == actorId)
                {
                    int lastIndex = _count - 1;
                    if (denseIndex != lastIndex)
                    {
                        uint lastKey = _dense[lastIndex];
                        _dense[denseIndex] = lastKey;
                        _sparse[lastKey] = slot;
                    }

                    _sparse[actorId] = 0;
                    _count = lastIndex;

                    Removed?.Invoke(new ActorId(actorId));
                    return true;
                }
            }
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryAddEntry(uint actorId)
    {
        if (actorId < (uint)_sparse.Length && (uint)_count < (uint)_dense.Length)
        {
            ref uint slot = ref _sparse[actorId];
            if (slot == 0)
            {
                var idx = (uint)_count;
                slot = idx + 1;
                _dense[idx] = actorId;
                _count++;

                Added?.Invoke(new ActorId(actorId));
                return true;
            }

            if (_dense[slot - 1] == actorId)
            {
                return false;
            }
        }

        return TryAddEntrySlow(actorId);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool TryAddEntrySlow(uint actorId)
    {
        EnsureCapacity(actorId);

        ref uint denseIndexPlusOne = ref _sparse[actorId];
        if (denseIndexPlusOne != 0)
        {
            if (_dense[denseIndexPlusOne - 1] == actorId)
            {
                return false;
            }
        }

        var denseIndex = (uint)_count;
        denseIndexPlusOne = denseIndex + 1;
        _dense[denseIndex] = actorId;
        _count++;

        Added?.Invoke(new ActorId(actorId));
        return true;
    }
}
