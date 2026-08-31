namespace Hexecsm;

public readonly ref struct ActorRef<T1, T2, T3>
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
{
    public static ActorRef<T1, T2, T3> Empty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new ActorRef<T1, T2, T3>();
    }

    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Id.IsEmpty;
    }

    public readonly ActorId Id;
    public readonly ref T1 Component1;
    public readonly ref T2 Component2;
    public readonly ref T3 Component3;

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ActorRef()
    {
        Id = ActorId.Empty;
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ActorRef(ActorId id, ref T1 component1, ref T2 component2, ref T3 component3)
    {
        Id = id;

        Component1 = ref component1;
        Component2 = ref component2;
        Component3 = ref component3;
    }

    #region Impplicit

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator bool(in ActorRef<T1, T2, T3> actor)
    {
        return actor.Id.IsNotEmpty;
    }

    #endregion
}
