using System.Runtime.InteropServices;

namespace Hexecs.Benchmarks.Collections;

public sealed class SparseDictionary<TValue> where TValue : struct
{
    private uint[] _sparse;
    private uint[] _dense;
    private TValue[] _values;
    private int _count;

    public SparseDictionary(int capacity = 64)
    {
        _sparse = new uint[capacity];
        _dense = new uint[capacity];
        _values = new TValue[capacity];
    }

    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _count;
    }

    public ReadOnlySpan<uint> Keys
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _dense.AsSpan(0, _count);
    }

    public ReadOnlySpan<TValue> Values
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _values.AsSpan(0, _count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(uint key)
    {
        uint[] sparse = _sparse;

        if ((uint)key < (uint)sparse.Length)
        {
            return sparse[key] != 0;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(uint key, out TValue value)
    {
        uint[] sparse = _sparse;

        if ((uint)key < (uint)sparse.Length)
        {
            uint denseIndexPlusOne = sparse[key];

            if (denseIndexPlusOne != 0)
            {
                value = _values[denseIndexPlusOne - 1];

                return true;
            }
        }

        value = default;

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(uint key, TValue value)
    {
        if (TryAdd(key, value))
        {
            return;
        }

        Throw("Key already exists");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryAdd(uint key, TValue value)
    {
        uint[] sparse = _sparse;

        // Проверка на наличие места и ключа в массиве
        if ((uint)key < (uint)sparse.Length && (uint)_count < (uint)_dense.Length)
        {
            ref uint slot = ref sparse[key];

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

        return TryAddSlow(key, value);
    }

    public bool Remove(uint key)
    {
        uint[] sparse = _sparse;

        if ((uint)key >= (uint)sparse.Length)
        {
            return false;
        }

        uint denseIndexPlusOne = sparse[key];

        if (denseIndexPlusOne == 0)
        {
            return false;
        }

        int denseIndex = (int)denseIndexPlusOne - 1;

        if (_dense[denseIndex] != key)
        {
            return false;
        }

        int lastIndex = _count - 1;

        if (denseIndex != lastIndex)
        {
            uint lastKey = _dense[lastIndex];
            _dense[denseIndex] = lastKey;
            _values[denseIndex] = _values[lastIndex];
            _sparse[lastKey] = (uint)denseIndex + 1;
        }

        sparse[key] = 0;
        _count = lastIndex;

        return true;
    }

    public void Clear()
    {
        uint[] dense = _dense;
        uint[] sparse = _sparse;

        for (var i = 0; i < _count; i++)
        {
            sparse[dense[i]] = 0;
        }

        _count = 0;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool TryAddSlow(uint key, TValue value)
    {
        if ((uint)key >= (uint)_sparse.Length)
        {
            int newSize = Math.Max(_sparse.Length * 2, (int)key + 1);
            Array.Resize(ref _sparse, newSize);
        }

        if ((uint)_count >= (uint)_dense.Length)
        {
            int newSize = _dense.Length * 2;
            Array.Resize(ref _dense, newSize);
            Array.Resize(ref _values, newSize);
        }

        ref uint denseIndexPlusOne = ref _sparse[key];

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator()
    {
        return new Enumerator(_values.AsSpan(0, _count));
    }

    public ref struct Enumerator
    {
        private ref TValue _current;
        private int _remaining;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(Span<TValue> values)
        {
            _remaining = values.Length;

            if (_remaining == 0)
            {
                _current = ref Unsafe.NullRef<TValue>();

                return;
            }

            ref TValue first = ref MemoryMarshal.GetReference(values);
            _current = ref Unsafe.Subtract(ref first, 1);
        }

        public ref TValue Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _current;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            if (--_remaining < 0)
            {
                return false;
            }

            _current = ref Unsafe.Add(ref _current, 1);

            return true;
        }
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Throw(string message)
    {
        throw new InvalidOperationException(message);
    }
}
