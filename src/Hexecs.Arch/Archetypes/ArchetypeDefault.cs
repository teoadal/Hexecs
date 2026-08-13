using Hexecs.Arch.Components;

namespace Hexecs.Arch.Archetypes;

internal sealed class ArchetypeDefault(ushort id) : IArchetype
{
    public ushort Id { get; } = id;

    public ArchetypeSign Sign { get; } = new();

    private readonly HashSet<uint> _actors = new();

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

    public bool Remove(uint actorId)
    {
        return _actors.Remove(actorId);
    }

    public bool TryGet<T>(uint actorId, out T component) where T : struct, IActorComponent
    {
        component = default;
        return false;
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