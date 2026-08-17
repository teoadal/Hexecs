using Hexecs.Arch.Components;

namespace Hexecs.Arch;

public readonly ref struct Actor<T1>
    where T1 : struct, IActorComponent
{
    public readonly ActorId Id;
    public readonly ref T1 Component1;
    
    internal Actor(ActorId id, ref T1 component1)
    {
        Id = id;
        Component1 = ref component1;
    }
}