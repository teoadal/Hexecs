using Hexecs.Arch.Components;

namespace Hexecs.Arch.Archetypes;

internal interface IArchetype
{
    ushort Id { get; }
    
    ArchetypeSign Sign { get; }
    
    bool AddFrom(IArchetype source, uint actorId);

    bool Contains(uint actorId);

    bool Remove(uint actorId);

    bool TryGet<T>(uint actorId, out T component)
        where T : struct, IActorComponent;

    ref T TryGetRef<T>(uint actorId)
        where T : struct, IActorComponent;

    bool Set<T>(uint actorId, ref readonly T component)
        where T : struct, IActorComponent;
}