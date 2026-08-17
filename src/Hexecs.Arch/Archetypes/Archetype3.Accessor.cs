using Hexecs.Arch.Components;

namespace Hexecs.Arch.Archetypes;

internal ref struct ArchetypeAccessor<T1, T2, T3>
    where T1 : struct, IActorComponent
    where T2 : struct, IActorComponent
    where T3 : struct, IActorComponent
{
    public static ArchetypeAccessor<T1, T2, T3> Empty(uint actorId) =>
        new(
            actorId,
            ref Unsafe.NullRef<T1>(),
            ref Unsafe.NullRef<T2>(),
            ref Unsafe.NullRef<T3>());

    public readonly uint ActorId;

    public ref T1 Component1;

    public ref T2 Component2;

    public ref T3 Component3;

    public ArchetypeAccessor(uint actorId, ref T1 component1, ref T2 component2, ref T3 component3)
    {
        ActorId = actorId;
        Component1 = ref component1;
        Component2 = ref component2;
        Component3 = ref component3;
    }
}