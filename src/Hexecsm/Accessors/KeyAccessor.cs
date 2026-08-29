namespace Hexecsm.Accessors;

public readonly ref struct KeyAccessor
{
    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _keys.Length;
    }

    private readonly ReadOnlySpan<ActorId> _keys;

    [SkipLocalsInit]
    internal KeyAccessor(ReadOnlySpan<ActorId> keys)
    {
        _keys = keys;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<ActorId> AsReadOnlySpan()
    {
        return _keys;
    }

    public ActorId this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _keys[index];
    }
}
