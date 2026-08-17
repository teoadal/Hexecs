using Hexecs.Arch.Components;

namespace Hexecs.Arch.Archetypes;

internal ref struct ArchetypeAccessor<T1>
    where T1 : struct, IActorComponent
{
    public static ArchetypeAccessor<T1> Empty(uint actorId) =>
        new(
            actorId,
            ref Unsafe.NullRef<T1>());

    public readonly uint ActorId;

    public ref T1 Component1;

    public ArchetypeAccessor(uint actorId, ref T1 component1)
    {
        ActorId = actorId;
        Component1 = ref component1;
    }
}