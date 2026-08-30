namespace Hexecsm;

public readonly ref struct ActorRef<T1>
    where T1 : struct, IComponent
{
    public readonly ActorId Id;
    public readonly ref T1 Component1;

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ActorRef(ActorId id, ref T1 component1)
    {
        Id = id;

        Component1 = ref component1;
    }
}
