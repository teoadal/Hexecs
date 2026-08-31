using Hexecsm.Accessors;

namespace Hexecsm.Utils;

internal sealed class ActorDictionary<TValue>(int initialCapacity)
    where TValue : struct
{
    private const uint EmptySlot = 0;

    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _count;
    }

    public KeyAccessor Keys
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new KeyAccessor(keys: new ReadOnlySpan<ActorId>(_dense, 0, _count));
    }

    public ValueAccessor<TValue> Values
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get =>
            new ValueAccessor<TValue>(
                mapping: _sparse,
                values: new Span<TValue>(_values, 0, _count));
    }

    private uint[] _sparse = new uint[initialCapacity];
    private ActorId[] _dense = new ActorId[initialCapacity];
    private TValue[] _values = new TValue[initialCapacity];
    private int _count = 0;

    public void Clear()
    {
        uint[] sparse = _sparse;
        ActorId[] dense = _dense;
        TValue[] values = _values;

        for (var i = 0; i < _count; i++)
        {
            sparse[dense[i].Value] = 0;
            values[i] = default;
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
    public KeyValueAccessor<TValue> GetAccessor()
    {
        return new KeyValueAccessor<TValue>(
            keys: new ReadOnlySpan<ActorId>(_dense, 0, _count),
            values: new Span<TValue>(_values, 0, _count));
    }

    public bool Remove(ActorId key, bool clear)
    {
        uint keyRaw = key.Value;
        uint[] sparse = _sparse;

        if ((uint)keyRaw < (uint)sparse.Length)
        {
            uint slot = sparse[keyRaw];

            if (slot != EmptySlot)
            {
                int denseIndex = (int)slot - 1;

                if (_dense[denseIndex] == key)
                {
                    if (clear)
                    {
                        ref TValue valueRef = ref _values[denseIndex];
                        valueRef = default;
                    }

                    int lastIndex = _count - 1;

                    if (denseIndex != lastIndex)
                    {
                        ActorId lastKey = _dense[lastIndex];
                        _dense[denseIndex] = lastKey;
                        _values[denseIndex] = _values[lastIndex];
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

    public bool Remove(ActorId key, bool clear, out TValue value)
    {
        uint keyRaw = key.Value;
        uint[] sparse = _sparse;

        if ((uint)keyRaw < (uint)sparse.Length)
        {
            uint slot = sparse[keyRaw];

            if (slot != EmptySlot)
            {
                int denseIndex = (int)slot - 1;

                if (_dense[denseIndex] == key)
                {
                    ref TValue valueRef = ref _values[denseIndex];
                    value = valueRef; // copy

                    if (clear)
                    {
                        valueRef = default;
                    }

                    int lastIndex = _count - 1;

                    if (denseIndex != lastIndex)
                    {
                        ActorId lastKey = _dense[lastIndex];
                        _dense[denseIndex] = lastKey;
                        _values[denseIndex] = _values[lastIndex];
                        _sparse[lastKey.Value] = slot;
                    }

                    _sparse[keyRaw] = 0;
                    _count = lastIndex;

                    return true;
                }
            }
        }

        value = default;

        return false;
    }

    /// <summary>
    /// Fast Path: если <paramref name="key"/> влезает в массив и есть место в dense
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryAdd(ActorId key, in TValue value)
    {
        uint keyRaw = key.Value;

        if (keyRaw < (uint)_sparse.Length && (uint)_count < (uint)_dense.Length)
        {
            ref uint slot = ref _sparse[keyRaw];

            if (slot == 0)
            {
                var idx = (uint)_count;
                slot = idx + 1;
                _dense[idx] = key;
                _values[idx] = value;

                _count++;

                return true;
            }

            if (_dense[slot - 1] == key)
            {
                return false;
            }
        }

        return TryAddSlow(key, in value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref TValue TryGetRef(ActorId key)
    {
        uint keyRaw = key.Value;
        uint[] sparse = _sparse;

        if (keyRaw < (uint)sparse.Length)
        {
            uint denseIndex = sparse[keyRaw];

            if (denseIndex != 0)
            {
                int index = (int)denseIndex - 1;

                if (_dense[index] == key)
                {
                    return ref _values[index];
                }
            }
        }

        return ref Unsafe.NullRef<TValue>();
    }

    private void EnsureCapacity(uint capacity)
    {
        // Проверка емкости плотных массивов (количество элементов)
        if (_count >= _dense.Length)
        {
            int newSize = _dense.Length * 2;
            Array.Resize(ref _dense, newSize);
            Array.Resize(ref _values, newSize);
        }

        // Проверка емкости разреженного массива (максимальный ID)
        if (capacity >= (uint)_sparse.Length)
        {
            uint newSize = Math.Max((uint)_sparse.Length * 2, capacity + 1);
            Array.Resize(ref _sparse, (int)newSize);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool TryAddSlow(ActorId key, in TValue value)
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
        _values[denseIndex] = value;

        _count++;

        return true;
    }
}
