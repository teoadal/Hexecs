using Hexecs.Arch.Archetypes;
using Hexecs.Arch.Components;

namespace Hexecs.Arch;

internal sealed class World
{
    private static byte _id = 1;
    private static Dictionary<byte, World> _worlds = new();

    public static World Create()
    {
        var id = _id++;
        var world = new World(id);

        _worlds.Add(id, world);

        return world;
    }

    public static World Get(byte id)
    {
        return _worlds[id];
    }

    public const byte DeletedWorldId = 0;

    public readonly byte Id;

    private readonly Dictionary<uint, Entry> _actors;

    private readonly Dictionary<ushort, IArchetype> _archetypes;
    private readonly ArchetypeFactory _archetypeFactory;
    private readonly ArchetypeDefault _defaultArchetype;

    private uint _nextActorId;
    private readonly Queue<uint> _freeActorIds;

    private World(byte id)
    {
        if (id == DeletedWorldId)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "World id can't be equal to 0");
        }

        Id = id;

        _actors = new Dictionary<uint, Entry>();

        _archetypeFactory = new ArchetypeFactory();
        _defaultArchetype = _archetypeFactory.CreateDefault();
        _archetypes = new Dictionary<ushort, IArchetype>
        {
            [_defaultArchetype.Id] = _defaultArchetype
        };

        _freeActorIds = new Queue<uint>();
    }

    public bool AddComponent<T>(
        ref ActorId actorId,
        ref readonly T component)
        where T : struct, IActorComponent
    {
        if (actorId.WorldId != Id)
        {
            return false;
        }

        var actorIdValue = actorId.Value;

        ref var entry = ref CollectionsMarshal.GetValueRefOrNullRef(_actors, actorIdValue);
        if (Unsafe.IsNullRef(ref entry) || entry.Version != actorId.Version)
        {
            return false;
        }

        if (!_archetypes.TryGetValue(entry.ArchetypeId, out var currentArchetype) ||
            currentArchetype.Contains<T>(actorIdValue))
        {
            return false;
        }

        var componentTypeId = ActorComponentType<T>.Id;

        var currentArchetypeSign = currentArchetype.Sign;
        if (currentArchetypeSign.Contains(componentTypeId))
        {
            return false;
        }

        var targetArchetype = GetArchetypeForAddTransitionFrom(in currentArchetypeSign, componentTypeId);
        if (!targetArchetype.AddFrom(currentArchetype, actorIdValue))
        {
            return false;
        }

        if (!targetArchetype.Set(actorIdValue, in component))
        {
            targetArchetype.Remove(actorIdValue);
            return false;
        }

        if (!currentArchetype.Remove(actorIdValue))
        {
            targetArchetype.Remove(actorIdValue);
            return false;
        }

        actorId = new ActorId(actorIdValue, ++entry.Version, Id);

        return true;
    }

    public ActorId CreateActor()
    {
        uint actorId;
        uint version;

        if (_freeActorIds.TryDequeue(out var freeId))
        {
            actorId = freeId;
            version = _actors[actorId].Version;
        }
        else
        {
            actorId = _nextActorId++;
            version = 0;
        }

        _defaultArchetype.Add(actorId);

        ref var entry = ref CollectionsMarshal.GetValueRefOrAddDefault(_actors, actorId, out _);
        entry.ArchetypeId = _defaultArchetype.Id;
        entry.Version = version;

        return new ActorId(actorId, version, Id);
    }

    public bool DestroyActor(ref ActorId actorId)
    {
        if (actorId.WorldId != Id)
        {
            return false;
        }

        var actorIdValue = actorId.Value;

        ref var entry = ref CollectionsMarshal.GetValueRefOrNullRef(_actors, actorIdValue);
        if (Unsafe.IsNullRef(ref entry))
        {
            return false;
        }

        if (!_archetypes.TryGetValue(entry.ArchetypeId, out var archetype) || !archetype.Remove(actorIdValue))
        {
            return false;
        }

        _freeActorIds.Enqueue(actorIdValue);

        entry.ArchetypeId = _defaultArchetype.Id;
        entry.Version++;

        actorId = new ActorId(actorIdValue, entry.Version, Id);

        return true;
    }

    public IEnumerable<Actor<T1, T2>> Filter<T1, T2>()
        where T1 : struct, IActorComponent
        where T2 : struct, IActorComponent
    {
        var componentType1Id = ActorComponentType<T1>.Id;
        var componentType2Id = ActorComponentType<T2>.Id;

        foreach (var archetype in _archetypes.Values)
        {
            if (!archetype.Sign.Contains(componentType1Id, componentType2Id))
            {
                continue;
            }

            foreach (var accessor in archetype.Filter<T1, T2>())
            {
                var actorId = accessor.ActorId;
                yield return new Actor<T1, T2>(
                    new ActorId(actorId, _actors[actorId].Version, Id),
                    ref accessor.Component1,
                    ref accessor.Component2);
            }
        }
    }

    public bool HasComponent<T>(in ActorId actorId)
        where T : struct, IActorComponent
    {
        if (actorId.WorldId != Id)
        {
            return false;
        }

        var actorIdValue = actorId.Value;

        ref var entry = ref CollectionsMarshal.GetValueRefOrNullRef(_actors, actorIdValue);

        return !Unsafe.IsNullRef(ref entry) &&
               _archetypes.TryGetValue(entry.ArchetypeId, out var archetype) &&
               archetype.Contains<T>(actorIdValue);
    }

    public bool RemoveComponent<T>(ref ActorId actorId)
        where T : struct, IActorComponent
    {
        if (actorId.WorldId != Id)
        {
            return false;
        }

        var actorIdValue = actorId.Value;

        ref var entry = ref CollectionsMarshal.GetValueRefOrNullRef(_actors, actorIdValue);
        if (Unsafe.IsNullRef(ref entry) && entry.Version != actorId.Version)
        {
            return false;
        }

        if (!_archetypes.TryGetValue(entry.ArchetypeId, out var currentArchetype) ||
            !currentArchetype.Contains<T>(actorIdValue))
        {
            return false;
        }

        var componentTypeId = ActorComponentType<T>.Id;
        var currentArchetypeSign = currentArchetype.Sign;

        if (!currentArchetypeSign.Contains(componentTypeId))
        {
            return false;
        }

        var targetArchetype = GetArchetypeForRemoveTransitionFrom(in currentArchetypeSign, componentTypeId);

        if (!targetArchetype.AddFrom(currentArchetype, actorIdValue))
        {
            return false;
        }

        if (!currentArchetype.Remove(actorIdValue))
        {
            targetArchetype.Remove(actorIdValue);
            return false;
        }

        entry.ArchetypeId = targetArchetype.Id;
        actorId = new ActorId(actorIdValue, ++entry.Version, Id);

        return true;
    }

    public bool TryGetComponent<T>(in ActorId actorId, out T component)
        where T : struct, IActorComponent
    {
        if (actorId.WorldId != Id)
        {
            component = default;
            return false;
        }

        var actorIdValue = actorId.Value;
        ref var entry = ref CollectionsMarshal.GetValueRefOrNullRef(_actors, actorIdValue);
        if (Unsafe.IsNullRef(ref entry))
        {
            component = default;
            return false;
        }

        if (!_archetypes.TryGetValue(entry.ArchetypeId, out var archetype))
        {
            component = default;
            return false;
        }

        return archetype.TryGet(actorIdValue, out component);
    }

    public ref T TryGetComponentRef<T>(in ActorId actorId)
        where T : struct, IActorComponent
    {
        if (actorId.WorldId != Id)
        {
            return ref Unsafe.NullRef<T>();
        }

        var actorIdValue = actorId.Value;
        ref var entry = ref CollectionsMarshal.GetValueRefOrNullRef(_actors, actorIdValue);
        if (Unsafe.IsNullRef(ref entry))
        {
            return ref Unsafe.NullRef<T>();
        }

        if (!_archetypes.TryGetValue(entry.ArchetypeId, out var archetype))
        {
            return ref Unsafe.NullRef<T>();
        }

        return ref archetype.TryGetRef<T>(actorIdValue);
    }

    private IArchetype GetArchetypeForAddTransitionFrom(
        in ArchetypeSign currentSign,
        ushort addComponentTypeId)
    {
        foreach (var existsArchetype in _archetypes.Values)
        {
            if (existsArchetype.Sign.IsAddTransitionFrom(currentSign, addComponentTypeId))
            {
                return existsArchetype;
            }
        }

        var newArchetype = _archetypeFactory.Create(currentSign.With(addComponentTypeId));
        _archetypes[newArchetype.Id] = newArchetype;

        return newArchetype;
    }

    private IArchetype GetArchetypeForRemoveTransitionFrom(
        in ArchetypeSign currentSign,
        ushort removeComponentTypeId)
    {
        if (currentSign.ComponentIds.Length == 1)
        {
            return _defaultArchetype;
        }

        foreach (var existsArchetype in _archetypes.Values)
        {
            if (existsArchetype.Sign.IsRemoveTransitionFrom(currentSign, removeComponentTypeId))
            {
                return existsArchetype;
            }
        }

        var newArchetype = _archetypeFactory.Create(currentSign.Without(removeComponentTypeId));
        _archetypes[newArchetype.Id] = newArchetype;

        return newArchetype;
    }

    private struct Entry
    {
        public ushort ArchetypeId;
        public uint Version;
    }
}