using Hexecs.Arch.Components;

namespace Hexecs.Arch.Archetypes;

internal sealed class Archetype<T1>(ushort id, int capacity) : ArchetypeBase<ArchetypeEntry<T1>>(
    id, capacity,
    new ArchetypeSign(
        ActorComponentType<T1>.Id))
    where T1 : struct, IActorComponent
{
    public override bool AddFrom(IArchetype source, uint actorId)
    {
        if (!TryAddEntry(actorId, out var reference))
        {
            return false;
        }

        ref var entry = ref reference.Value;

        entry.Id = actorId;

        var sourceAccessor = source.TryGetAccessor<T1>(actorId);

        entry.Component1 = Unsafe.IsNullRef(ref sourceAccessor.Component1)
            ? default
            : sourceAccessor.Component1;

        return true;
    }
}