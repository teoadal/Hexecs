namespace Hexecsm.Accessors;

public readonly ref struct KeyValueAccessor<TValue>
    where TValue : struct
{
    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _values.Length;
    }

    private readonly ReadOnlySpan<ActorId> _keys;
    private readonly Span<TValue> _values;

    [SkipLocalsInit]
    internal KeyValueAccessor(
        ReadOnlySpan<ActorId> keys,
        Span<TValue> values)
    {
        _keys = keys;
        _values = values;
    }

    public KeyValueRef<TValue> this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new KeyValueRef<TValue>(_keys[index], ref _values[index]);
    }
}

public readonly ref struct KeyValueRef<TValue>
    where TValue : struct
{
    public readonly ActorId Key;
    public readonly ref TValue Value;

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public KeyValueRef(ActorId key, ref TValue value)
    {
        Key = key;
        Value = ref value;
    }
}
