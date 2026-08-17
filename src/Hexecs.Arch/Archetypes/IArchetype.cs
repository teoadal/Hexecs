using Hexecs.Arch.Components;

namespace Hexecs.Arch.Archetypes;

internal interface IArchetype
{
    ushort Id { get; }

    int Length { get; }

    ArchetypeSign Sign { get; }

    bool AddFrom(IArchetype source, uint actorId);

    bool Contains(uint actorId);

    /// <summary>
    /// Check is actor with a component of type <see cref="T"/> exists in current archetype.
    /// </summary>
    bool Contains<T>(uint actorId)
        where T : struct, IActorComponent;

    IEnumerable<ArchetypeAccessor<T1, T2>> Filter<T1, T2>()
        where T1 : struct, IActorComponent
        where T2 : struct, IActorComponent;

    IEnumerable<ArchetypeAccessor<T1, T2, T3>> Filter<T1, T2, T3>()
        where T1 : struct, IActorComponent
        where T2 : struct, IActorComponent
        where T3 : struct, IActorComponent;

    bool Remove(uint actorId);

    bool Remove<T>(uint actorId, out T component)
        where T : struct, IActorComponent;

    bool TryGet<T>(uint actorId, out T component)
        where T : struct, IActorComponent;

    ArchetypeAccessor<T1> TryGetAccessor<T1>(uint actorId)
        where T1 : struct, IActorComponent;
    
    ArchetypeAccessor<T1, T2> TryGetAccessor<T1, T2>(uint actorId)
        where T1 : struct, IActorComponent
        where T2 : struct, IActorComponent;

    ArchetypeAccessor<T1, T2, T3> TryGetAccessor<T1, T2, T3>(uint actorId)
        where T1 : struct, IActorComponent
        where T2 : struct, IActorComponent
        where T3 : struct, IActorComponent;

    ref T TryGetRef<T>(uint actorId)
        where T : struct, IActorComponent;

    bool Set<T>(uint actorId, ref readonly T component)
        where T : struct, IActorComponent;
}