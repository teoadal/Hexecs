namespace Hexecsm.Accessors;

public readonly ref struct ValueAccessor<TValue>
    where TValue : struct
{
    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _values.Length;
    }

    private readonly uint[] _mapping;
    private readonly Span<TValue> _values;

    [SkipLocalsInit]
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

    public ref TValue GetValue(ActorId actorId)
    {
        uint keyRaw = actorId.Value;

        if (keyRaw < (uint)_mapping.Length)
        {
            uint denseIndexPlusOne = _mapping[keyRaw];

            if (denseIndexPlusOne != 0)
            {
                int index = (int)denseIndexPlusOne - 1;

                if (index < _values.Length)
                {
                    return ref _values[index];
                }
            }
        }

        return ref Unsafe.NullRef<TValue>();
    }

    public ref TValue this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _values[index];
    }
}
