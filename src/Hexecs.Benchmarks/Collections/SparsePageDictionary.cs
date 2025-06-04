using System.Runtime.CompilerServices;
using Hexecs.Utils;

namespace Hexecs.Benchmarks.Collections;

public sealed class SparsePageDictionary<TValue>
    where TValue : struct
{
    private const int PageBits = 12;
    private const int PageSize = 1 << PageBits; // 4096
    private const int PageMask = PageSize - 1;

    private uint[]?[] _sparsePages;
    private uint[] _dense;
    private TValue[] _values;
    private int _count;

    public SparsePageDictionary(int initialPageCount = 16, int denseCapacity = 64)
    {
        _sparsePages = new uint[initialPageCount][];
        _dense = new uint[denseCapacity];
        _values = new TValue[denseCapacity];
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
        var pageIndex = (int)(key >> PageBits);
        var pages = _sparsePages;

        if ((uint)pageIndex < (uint)pages.Length)
        {
            var page = pages[pageIndex];
            if (page != null)
            {
                var denseIndexPlusOne = page[key & PageMask];
                return denseIndexPlusOne != 0 && _dense[denseIndexPlusOne - 1] == key;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(uint key, out TValue value)
    {
        var pageIndex = (int)(key >> PageBits);
        if ((uint)pageIndex < (uint)_sparsePages.Length)
        {
            var page = _sparsePages[pageIndex];
            if (page != null)
            {
                var denseIndexPlusOne = page[key & PageMask];
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
        }
        value = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(uint key, TValue value)
    {
        if (TryAdd(key, value)) return;
        ActorError.AlreadyExists(key); // выбрасывает ошибку
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryAdd(uint key, TValue value)
    {
        var pageIndex = (int)(key >> PageBits);
        var pages = _sparsePages;

        // Максимально компактная проверка на готовность страницы и места
        if ((uint)pageIndex < (uint)pages.Length)
        {
            var page = pages[pageIndex];
            if (page != null && (uint)_count < (uint)_dense.Length)
            {
                ref var slot = ref page[key & PageMask];
                if (slot == 0) // Чистая вставка (самый частый случай в ECS)
                {
                    var idx = (uint)_count;
                    slot = idx + 1;
                    _dense[idx] = key;
                    _values[idx] = value;
                    _count++;
                    return true;
                }

                // Если не 0, проверяем на дубликат (чуть медленнее)
                if (_dense[slot - 1] == key) return false;
            }
        }

        return TryAddSlow(key, value);
    }

    public bool Remove(uint key)
    {
        var pageIndex = (int)(key >> PageBits);
        // Используем вашу оригинальную "плоскую" логику из коммита
        if ((uint)pageIndex >= (uint)_sparsePages.Length) return false;

        var page = _sparsePages[pageIndex];
        if (page == null) return false;

        var offset = (int)(key & PageMask);
        var denseIndexPlusOne = page[offset];
        if (denseIndexPlusOne == 0) return false;

        var denseIndex = (int)denseIndexPlusOne - 1;
        if (_dense[denseIndex] != key) return false;

        var lastIndex = _count - 1;
        if (denseIndex != lastIndex)
        {
            var lastKey = _dense[lastIndex];
            _dense[denseIndex] = lastKey;
            _values[denseIndex] = _values[lastIndex];

            var lastKeyPageIndex = (int)(lastKey >> PageBits);
            _sparsePages[lastKeyPageIndex]![lastKey & PageMask] = (uint)denseIndex + 1;
        }

        page[offset] = 0;
        _count = lastIndex; // Используем вычисленный lastIndex
        return true;
    }

    public void Clear()
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

    private void EnsurePageArraySize(int pageIndex)
    {
        if (pageIndex >= _sparsePages.Length)
        {
            var newSize = Math.Max(_sparsePages.Length * 2, pageIndex + 1);
            Array.Resize(ref _sparsePages, newSize);
        }
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

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool TryAddSlow(uint key, TValue value)
    {
        EnsureDenseCapacity();
        var pageIndex = (int)(key >> PageBits);
        EnsurePageArraySize(pageIndex);

        ref var page = ref _sparsePages[pageIndex];
        if (page == null)
        {
            page = ArrayUtils.Create<uint>(PageSize);
            Array.Clear(page, 0, page.Length);
        }

        ref var denseIndexPlusOne = ref page[key & PageMask];
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
}