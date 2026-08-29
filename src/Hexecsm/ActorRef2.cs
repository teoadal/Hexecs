namespace Hexecsm;

public readonly ref struct ActorRef<T1, T2>
    where T1 : struct, IComponent
    where T2 : struct, IComponent
{
    public readonly ActorId Id;
    public readonly ref T1 Component1;
    public readonly ref T2 Component2;

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ActorRef(ActorId id, ref T1 component1, ref T2 component2)
    {
        Id = id;
        Component2 = ref component2;
        Component1 = ref component1;
    }
}
