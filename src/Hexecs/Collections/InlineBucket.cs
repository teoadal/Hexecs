namespace Hexecs.Collections;

[DebuggerDisplay("Length = {Length}")]
[DebuggerTypeProxy(typeof(InlineBucket<>.InlineBucketDebugProxy))]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
internal struct InlineBucket<T>()
    where T : struct
{
    public const int InlineArraySize = 10;

    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _length;
    }

    private InlineItemArray _inlineArray;
    private T[] _array = [];
    private int _length = 0;

    public void Add(T item)
    {
        if (_length < InlineArraySize) _inlineArray[_length] = item;
        else ArrayUtils.Insert(ref _array, ArrayPool<T>.Shared, _length - InlineArraySize, item);

        _length++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Contains(T item, EqualityComparer<T>? equalityComparer = null)
    {
        return IndexOf(item, equalityComparer) > -1;
    }

    public readonly void CopyTo(ref Span<T> buffer)
    {
        if (_length == 0) return;

        for (var i = 0; i < _length; i++)
        {
            buffer[i] = this[i];
        }
    }

    public void Dispose()
    {
        if (_array is { Length: > 0 }) ArrayPool<T>.Shared.Return(_array);
        _array = [];
        _length = 0;
    }

    public Enumerator GetEnumerator()
    {
        ref var reference = ref Unsafe.As<InlineItemArray, T>(ref _inlineArray);
        var span = MemoryMarshal.CreateSpan(ref reference, InlineArraySize);
        return new Enumerator(span, _array, _length);
    }

    public ref T GetRef(int index)
    {
        if (index >= InlineArraySize) return ref _array[index - InlineArraySize];

        ref var reference = ref Unsafe.As<InlineItemArray, T>(ref _inlineArray);
        var span = MemoryMarshal.CreateSpan(ref reference, InlineArraySize);
        return ref span[index];
    }

    public readonly int IndexOf(T item, EqualityComparer<T>? equalityComparer = null)
    {
        if (_length == 0) return -1;

        equalityComparer ??= EqualityComparer<T>.Default;

        var inlineLength = Math.Min(_length, InlineArraySize);
        for (var i = 0; i < inlineLength; i++)
        {
            if (equalityComparer.Equals(_inlineArray[i], item))
            {
                return i;
            }
        }

        if (_array == null || _array.Length == 0) return -1;

        var span = _array.AsSpan(0, _length - InlineArraySize);
        for (var i = 0; i < span.Length; i++)
        {
            if (equalityComparer.Equals(span[i], item)) return InlineArraySize + i;
        }

        return -1;
    }

    public bool Remove(T item, EqualityComparer<T>? equalityComparer = null)
    {
        if (_length == 0) return false;

        equalityComparer ??= EqualityComparer<T>.Default;

        var arraySize = _length - InlineArraySize;
        var inlineLength = Math.Min(_length, InlineArraySize);

        for (var index = 0; index < inlineLength; index++)
        {
            if (!equalityComparer.Equals(_inlineArray[index], item)) continue;

            var inlineEnd = inlineLength - 1;
            for (var i = index; i < inlineEnd; i++)
            {
                _inlineArray[i] = _inlineArray[i + 1];
            }

            if (arraySize > 0)
            {
                const int lastInlineIndex = InlineArraySize - 1;
                _inlineArray[lastInlineIndex] = _array[0];
                ArrayUtils.Cut(_array, 0, arraySize);
            }

            _length--;
            return true;
        }

        if (_array.Length == 0 || arraySize <= 0) return false;

        var span = _array.AsSpan(0, arraySize);
        for (var i = 0; i < span.Length; i++)
        {
            if (!equalityComparer.Equals(span[i], item)) continue;
            ArrayUtils.Cut(_array, i, arraySize);
            _length--;

            return true;
        }

        return false;
    }

    public T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => index < InlineArraySize ? _inlineArray[index] : _array[index - InlineArraySize];
        set
        {
            if (index < InlineArraySize) _inlineArray[index] = value;
            else _array[index - InlineArraySize] = value;
        }
    }

    public readonly T[] ToArray()
    {
        if (_length == 0) return [];

        var result = ArrayUtils.Create<T>(_length);
        for (var i = 0; i < _length; i++)
        {
            result[i] = this[i];
        }

        return result;
    }

    public bool TryAdd(T item, EqualityComparer<T>? equalityComparer = null)
    {
        var has = Contains(item, equalityComparer);
        if (has) return false;

        Add(item);
        return true;
    }

    public ref struct Enumerator
    {
        public readonly ref T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _index < InlineArraySize
                ? ref _inlineArray[_index]
                : ref _array[_index - InlineArraySize];
        }

        private readonly Span<T> _inlineArray;
        private readonly T[] _array;
        private readonly int _length;
        private int _index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(Span<T> inlineArray, T[] array, int length)
        {
            _inlineArray = inlineArray;
            _array = array;
            _length = length;
            _index = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() => ++_index < _length;
    }

    [InlineArray(InlineArraySize)]
    private struct InlineItemArray
    {
        private T _item;
    }

    private sealed class InlineBucketDebugProxy(InlineBucket<T> bucket)
    {
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public readonly T[] Elements = bucket.ToArray();
    }
}