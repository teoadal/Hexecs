using System.Runtime.CompilerServices;

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
        var sparse = _sparse;
        if ((uint)key < (uint)sparse.Length)
        {
            var denseIndexPlusOne = sparse[key];
            return denseIndexPlusOne != 0 && _dense[denseIndexPlusOne - 1] == key;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(uint key, out TValue value)
    {
        var sparse = _sparse;
        if ((uint)key < (uint)sparse.Length)
        {
            var denseIndexPlusOne = sparse[key];
            if (denseIndexPlusOne != 0)
            {
                var index = (int)denseIndexPlusOne - 1;
                if (_dense[index] == key)
                {
                    value = _values[index];
                    return true;
                }
            }
        }
        value = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(uint key, TValue value)
    {
        if (TryAdd(key, value)) return;
        // Здесь можно добавить ваш ActorError.AlreadyExists(key)
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryAdd(uint key, TValue value)
    {
        var sparse = _sparse;
        // Проверка на наличие места и ключа в массиве
        if ((uint)key < (uint)sparse.Length && (uint)_count < (uint)_dense.Length)
        {
            ref var slot = ref sparse[key];
            if (slot == 0)
            {
                var idx = (uint)_count;
                slot = idx + 1;
                _dense[idx] = key;
                _values[idx] = value;
                _count++;
                return true;
            }

            if (_dense[slot - 1] == key) return false;
        }

        return TryAddSlow(key, value);
    }

    public bool Remove(uint key)
    {
        var sparse = _sparse;
        if ((uint)key >= (uint)sparse.Length) return false;

        var denseIndexPlusOne = sparse[key];
        if (denseIndexPlusOne == 0) return false;

        var denseIndex = (int)denseIndexPlusOne - 1;
        if (_dense[denseIndex] != key) return false;

        var lastIndex = _count - 1;
        if (denseIndex != lastIndex)
        {
            var lastKey = _dense[lastIndex];
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
        var dense = _dense;
        var sparse = _sparse;

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
            var newSize = Math.Max(_sparse.Length * 2, (int)key + 1);
            Array.Resize(ref _sparse, newSize);
        }

        if ((uint)_count >= (uint)_dense.Length)
        {
            var newSize = _dense.Length * 2;
            Array.Resize(ref _dense, newSize);
            Array.Resize(ref _values, newSize);
        }

        ref var denseIndexPlusOne = ref _sparse[key];
        if (denseIndexPlusOne != 0)
        {
            if (_dense[denseIndexPlusOne - 1] == key) return false;
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
        var count = _count;
        return new Enumerator(
            _dense.AsSpan(0, count),
            _values.AsSpan(0, count));
    }

    public ref struct Enumerator
    {
        private readonly ReadOnlySpan<uint> _keys;
        private readonly Span<TValue> _values;
        private int _index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(ReadOnlySpan<uint> keys, Span<TValue> values)
        {
            _keys = keys;
            _values = values;
            _index = -1;
        }

        public ref TValue Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _values[_index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() => ++_index < _keys.Length;
    }
}