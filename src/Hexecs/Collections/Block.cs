using Hexecs.Actors;
using Hexecs.Assets;

namespace Hexecs.Collections;

/// <summary>
/// Неизменяемая коллекция элементов.
/// </summary>
/// <typeparam name="T">Тип элементов коллекции</typeparam>
[DebuggerDisplay("Length = {Length}")]
public readonly struct Block<T> : IEnumerable<T>, IActorComponent, IAssetComponent
{
    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _array == null || _array.Length == 0;
    }

    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _array?.Length ?? 0;
    }

    private readonly T[]? _array;

    #region Constructors

    public Block()
    {
        _array = [];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Block(T item)
    {
        _array = [item];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Block(T[] array) => _array = array;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Block(T[] array, int length)
    {
        _array = length <= 0
            ? []
            : array.AsSpan(0, length).ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Block(ReadOnlySpan<T> span) => _array = span.ToArray();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Block(List<T> collection) => _array = collection.ToArray();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Block(IEnumerable<T> collection) => _array = CollectionUtils.ToArray(collection);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Block(IEnumerable<T> collection, int length) => _array = CollectionUtils.ToArray(collection, length);

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Memory<T> AsMemory() => _array == null || _array.Length == 0
        ? Memory<T>.Empty
        : new Memory<T>(_array);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan() => _array == null || _array.Length == 0
        ? Span<T>.Empty
        : new Span<T>(_array);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(T value, IEqualityComparer<T>? equalityComparer = null)
    {
        return IndexOf(value, equalityComparer) > -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T GetRef(int index)
    {
        if (_array == null) return ref Unsafe.NullRef<T>();
        return ref _array[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ArrayEnumerator<T> GetEnumerator() => _array == null || _array.Length == 0
        ? ArrayEnumerator<T>.Empty
        : new ArrayEnumerator<T>(_array);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T[] GetUnderlyingArray() => _array ?? [];

    public int IndexOf(T value, IEqualityComparer<T>? equalityComparer = null)
    {
        if (_array == null || _array.Length == 0) return -1;

        equalityComparer ??= EqualityComparer<T>.Default;
        for (var i = 0; i < _array.Length; i++)
        {
            if (equalityComparer.Equals(_array[i], value)) return i;
        }

        return -1;
    }

    public T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _array![index];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _array![index] = value;
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}