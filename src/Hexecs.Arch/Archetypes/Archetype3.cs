using Hexecs.Arch.Components;

namespace Hexecs.Arch.Archetypes;

internal sealed class Archetype<T1, T2, T3>(ushort id, int capacity) : ArchetypeBase<ArchetypeEntry<T1, T2, T3>>(
    id, capacity,
    new ArchetypeSign(
        ActorComponentType<T1>.Id,
        ActorComponentType<T2>.Id,
        ActorComponentType<T3>.Id))
    where T1 : struct, IActorComponent
    where T2 : struct, IActorComponent
    where T3 : struct, IActorComponent
{
    public override bool AddFrom(IArchetype source, uint actorId)
    {
        if (!TryAddEntry(actorId, out var reference))
        {
            return false;
        }

        ref var entry = ref reference.Value;

        entry.Id = actorId;

        var sourceAccessor = source.TryGetAccessor<T1, T2, T3>(actorId);

        entry.Component1 = Unsafe.IsNullRef(ref sourceAccessor.Component1)
            ? default
            : sourceAccessor.Component1;

        entry.Component2 = Unsafe.IsNullRef(ref sourceAccessor.Component2)
            ? default
            : sourceAccessor.Component2;

        entry.Component3 = Unsafe.IsNullRef(ref sourceAccessor.Component3)
            ? default
            : sourceAccessor.Component3;

        return true;
    }
}