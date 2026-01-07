using Hexecs.Actors;
using Hexecs.Assets;

namespace Hexecs.Collections;

/// <summary>
/// Изменяемая коллекция элементов.
/// Использует <see cref="ArrayPool{T}"/> для расширения.
/// </summary>
/// <param name="capacity">Размер при инициализации</param>
/// <typeparam name="T">Тип элементов коллекции</typeparam>
[DebuggerDisplay("Length = {Length}")]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
internal struct Bucket<T>(int capacity) : IEnumerable<T>, IActorComponent, IAssetComponent
{
    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _array == null || _array.Length == 0;
    }

    // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
    public readonly int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _length;
    }

    private T[]? _array = capacity > 0 ? ArrayPool<T>.Shared.Rent(capacity) : null;
    private int _length = 0;

    public void Add(T item)
    {
        ArrayUtils.InsertOrCreate(ref _array, ArrayPool<T>.Shared, _length, in item);
        _length++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Memory<T> AsMemory() => _array == null || _array.Length == 0
        ? Memory<T>.Empty
        : new Memory<T>(_array);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Span<T> AsSpan() => _length == 0
        ? Span<T>.Empty
        : _array.AsSpan(0, _length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ReadOnlySpan<T> AsReadOnlySpan() => _length == 0
        ? ReadOnlySpan<T>.Empty
        : new ReadOnlySpan<T>(_array, 0, _length);

    public void Dispose()
    {
        if (_array is { Length: > 0 }) ArrayPool<T>.Shared.Return(_array);

        _array = [];
        _length = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ArrayEnumerator<T> GetEnumerator() => _array == null
        ? ArrayEnumerator<T>.Empty
        : new ArrayEnumerator<T>(_array, _length);

    public readonly int IndexOf(T item, IEqualityComparer<T>? equalityComparer = null)
    {
        if (_length == 0) return -1;

        equalityComparer ??= EqualityComparer<T>.Default;

        var span = AsReadOnlySpan();
        for (var i = 0; i < span.Length; i++)
        {
            if (equalityComparer.Equals(span[i], item)) return i;
        }

        return -1;
    }

    public readonly bool Has(T item, IEqualityComparer<T>? equalityComparer = null)
    {
        if (_length == 0) return false;

        equalityComparer ??= EqualityComparer<T?>.Default;

        foreach (var exists in AsReadOnlySpan())
        {
            if (equalityComparer.Equals(exists, item)) return true;
        }

        return false;
    }

    public bool Remove(T item, IEqualityComparer<T>? equalityComparer = null)
    {
        var index = IndexOf(item, equalityComparer);
        if (index == -1) return false;

        RemoveAtSwapBack(index);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveAtSwapBack(int index)
    {
        var lastIndex = _length - 1;
        if (index < lastIndex)
        {
            _array![index] = _array[lastIndex];
        }

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            _array![lastIndex] = default!;
        }

        _length--;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly T[] ToArray() => AsReadOnlySpan().ToArray();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Block<T> ToBlock() => new(AsReadOnlySpan());

    public bool TryAdd(T item, IEqualityComparer<T>? equalityComparer = null)
    {
        var has = Has(item, equalityComparer);
        if (has) return false;

        Add(item);
        return true;
    }

    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}