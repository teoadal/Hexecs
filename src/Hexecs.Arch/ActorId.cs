namespace Hexecs.Arch;

public readonly struct ActorId
{
    private readonly ulong _packed;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ActorId(uint value, uint version, byte worldId)
    {
        _packed = value |
                  ((ulong)(version & 0x00FF_FFFF) << 32) |
                  ((ulong)worldId << 56);
    }

    public uint Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (uint)_packed;
    }

    public uint Version
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (uint)((_packed >> 32) & 0x00FF_FFFFUL);
    }

    public byte WorldId
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (byte)(_packed >> 56);
    }
}