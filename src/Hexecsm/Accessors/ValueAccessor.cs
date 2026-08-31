namespace Hexecsm.Accessors;

public readonly ref struct ValueAccessor<TValue>
    where TValue : struct
{
    public static ValueAccessor<TValue> Empty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new ValueAccessor<TValue>([], Span<TValue>.Empty);
    }

    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _values.Length;
    }

    private readonly uint[] _mapping;
    private readonly Span<TValue> _values;

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ValueAccessor(uint[] mapping, Span<TValue> values)
    {
        _mapping = mapping;
        _values = values;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<TValue> AsReadOnlySpan()
    {
        return _values;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<TValue> AsSpan()
    {
        return _values;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref TValue GetValue(ActorId actorId)
    {
        return ref _values[(int)_mapping[actorId.Value - 1]];
    }

    public ref TValue this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _values[index];
    }
}
