using Hexecs.Arch.Components;

namespace Hexecs.Arch.Archetypes;

internal sealed class ArchetypeDefault(ushort id) : IArchetype
{
    public ushort Id { get; } = id;

    public int Length => _actors.Count;

    public IEnumerable<uint> Actors => _actors;

    public ArchetypeSign Sign { get; } = new();

    private readonly HashSet<uint> _actors = [];

    public bool Add(uint actorId)
    {
        return _actors.Add(actorId);
    }

    public bool AddFrom(IArchetype source, uint actorId)
    {
        return _actors.Add(actorId);
    }

    public bool Contains(uint actorId)
    {
        return _actors.Contains(actorId);
    }

    public bool Contains<T>(uint actorId) where T : struct, IActorComponent
    {
        return false;
    }

    public IEnumerable<ArchetypeAccessor<T1, T2>> Filter<T1, T2>()
        where T1 : struct, IActorComponent
        where T2 : struct, IActorComponent
    {
        return [];
    }

    public IEnumerable<ArchetypeAccessor<T1, T2, T3>> Filter<T1, T2, T3>()
        where T1 : struct, IActorComponent
        where T2 : struct, IActorComponent
        where T3 : struct, IActorComponent
    {
        return [];
    }

    public bool Remove(uint actorId)
    {
        return _actors.Remove(actorId);
    }

    public bool Remove<T>(uint actorId, out T component)
        where T : struct, IActorComponent
    {
        component = default;
        return false;
    }

    public bool TryGet<T>(uint actorId, out T component)
        where T : struct, IActorComponent
    {
        component = default;
        return false;
    }

    public ArchetypeAccessor<T1> TryGetAccessor<T1>(uint actorId) where T1 : struct, IActorComponent
    {
        return ArchetypeAccessor<T1>.Empty(actorId);
    }

    public ArchetypeAccessor<T1, T2> TryGetAccessor<T1, T2>(uint actorId) where T1 : struct, IActorComponent
        where T2 : struct, IActorComponent
    {
        return ArchetypeAccessor<T1, T2>.Empty(actorId);
    }

    public ArchetypeAccessor<T1, T2, T3> TryGetAccessor<T1, T2, T3>(uint actorId)
        where T1 : struct, IActorComponent
        where T2 : struct, IActorComponent
        where T3 : struct, IActorComponent
    {
        return ArchetypeAccessor<T1, T2, T3>.Empty(actorId);
    }

    public ref T TryGetRef<T>(uint actorId) where T : struct, IActorComponent
    {
        return ref Unsafe.NullRef<T>();
    }

    public bool Set<T>(uint actorId, ref readonly T component) where T : struct, IActorComponent
    {
        return false;
    }
}