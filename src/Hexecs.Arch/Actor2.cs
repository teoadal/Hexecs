using Hexecs.Arch.Components;

namespace Hexecs.Arch;

public readonly ref struct Actor<T1, T2>
    where T1 : struct, IActorComponent
    where T2 : struct, IActorComponent
{
    public readonly ActorId Id;
    public readonly ref T1 Component1;
    public readonly ref T2 Component2;
    
    internal Actor(ActorId id, ref T1 component1, ref T2 component2)
    {
        Id = id;
        Component1 = ref component1;
        Component2 = ref component2;
    }
}