using Hexecs.Arch.Components;

namespace Hexecs.Arch.Archetypes;

internal ref struct ArchetypeAccessor<T1, T2>
    where T1 : struct, IActorComponent
    where T2 : struct, IActorComponent
{
    public static ArchetypeAccessor<T1, T2> Empty(uint actorId) =>
        new(
            actorId,
            ref Unsafe.NullRef<T1>(),
            ref Unsafe.NullRef<T2>());
    
    public readonly uint ActorId;

    public ref T1 Component1;

    public ref T2 Component2;

    public ArchetypeAccessor(uint actorId, ref T1 component1, ref T2 component2)
    {
        ActorId = actorId;
        Component1 = ref component1;
        Component2 = ref component2;
    }
}