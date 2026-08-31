using Hexecsm.Accessors;

namespace Hexecsm.Utils;

internal sealed class ActorHashSet(int initialCapacity)
{
    private const uint EmptySlot = 0;

    public KeyAccessor Keys
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new KeyAccessor(keys: new ReadOnlySpan<ActorId>(_dense, 0, _count));
    }

    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _count;
    }

    private uint[] _sparse = new uint[initialCapacity];
    private ActorId[] _dense = new ActorId[initialCapacity];
    private int _count = 0;

    public void Clear()
    {
        uint[] sparse = _sparse;
        ActorId[] dense = _dense;

        for (var i = 0; i < _count; i++)
        {
            sparse[dense[i].Value] = 0;
        }

        _count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(ActorId key)
    {
        uint keyRaw = key.Value;
        uint[] sparse = _sparse;

        if (keyRaw < (uint)sparse.Length)
        {
            uint slot = sparse[keyRaw];

            return slot != EmptySlot && _dense[slot - 1] == key;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public KeyAccessor GetKeys(int start, int length)
    {
        return new KeyAccessor(
            keys: new ReadOnlySpan<ActorId>(
                _dense,
                start: start,
                length: length));
    }

    public bool Remove(ActorId key)
    {
        uint keyRaw = key.Value;
        uint[] sparse = _sparse;

        if ((uint)keyRaw < (uint)sparse.Length)
        {
            uint slot = sparse[keyRaw];

            if (slot != 0)
            {
                int denseIndex = (int)slot - 1;

                if (_dense[denseIndex] == key)
                {
                    int lastIndex = _count - 1;

                    if (denseIndex != lastIndex)
                    {
                        ActorId lastKey = _dense[lastIndex];
                        _dense[denseIndex] = lastKey;
                        _sparse[lastKey.Value] = slot;
                    }

                    _sparse[keyRaw] = 0;
                    _count = lastIndex;

                    return true;
                }
            }
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryAdd(ActorId key)
    {
        uint keyRaw = key.Value;

        if ((uint)keyRaw < (uint)_sparse.Length && (uint)_count < (uint)_dense.Length)
        {
            ref uint slot = ref _sparse[keyRaw];

            if (slot == 0)
            {
                var idx = (uint)_count;
                slot = idx + 1;
                _dense[idx] = key;

                _count++;

                return true;
            }

            if (_dense[slot - 1] == key)
            {
                return false;
            }
        }

        return TryAddSlow(key);
    }

    private void EnsureCapacity(uint capacity)
    {
        if (_count >= _dense.Length)
        {
            int newSize = _dense.Length * 2;
            Array.Resize(ref _dense, newSize);
        }

        if (capacity >= (uint)_sparse.Length)
        {
            uint newSize = Math.Max((uint)_sparse.Length * 2, capacity + 1);
            Array.Resize(ref _sparse, (int)newSize);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool TryAddSlow(ActorId key)
    {
        uint keyRaw = key.Value;

        EnsureCapacity(keyRaw);

        ref uint denseIndexPlusOne = ref _sparse[keyRaw];

        if (denseIndexPlusOne != 0)
        {
            if (_dense[denseIndexPlusOne - 1] == key)
            {
                return false;
            }
        }

        var denseIndex = (uint)_count;

        denseIndexPlusOne = denseIndex + 1;

        _dense[denseIndex] = key;

        _count++;

        return true;
    }
}
