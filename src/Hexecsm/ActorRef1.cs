namespace Hexecsm;

[DebuggerDisplay("Id = {Id.Value}")]
public readonly ref struct ActorRef<T1>
    where T1 : struct, IComponent
{
    public static ActorRef<T1> Empty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new ActorRef<T1>();
    }

    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Id.IsEmpty;
    }

    public readonly ActorId Id;
    public readonly ref T1 Component1;

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ActorRef()
    {
        Id = ActorId.Empty;
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ActorRef(ActorId id, ref T1 component1)
    {
        Id = id;

        Component1 = ref component1;
    }

    #region Implicit

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator bool(in ActorRef<T1> actor)
    {
        return actor.Id.IsNotEmpty;
    }

    #endregion
}
