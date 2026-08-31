namespace Hexecsm;

public readonly ref struct ActorRef<T1, T2>
    where T1 : struct, IComponent
    where T2 : struct, IComponent
{
    public static ActorRef<T1, T2> Empty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new ActorRef<T1, T2>();
    }

    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Id.IsEmpty;
    }

    public readonly ActorId Id;
    public readonly ref T1 Component1;
    public readonly ref T2 Component2;

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ActorRef()
    {
        Id = ActorId.Empty;
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ActorRef(ActorId id, ref T1 component1, ref T2 component2)
    {
        Id = id;
        Component2 = ref component2;
        Component1 = ref component1;
    }

    #region Implicit

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator bool(in ActorRef<T1, T2> actor)
    {
        return actor.Id.IsNotEmpty;
    }

    #endregion
}
