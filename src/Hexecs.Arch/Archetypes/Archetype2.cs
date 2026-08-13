using Hexecs.Arch.Components;

namespace Hexecs.Arch.Archetypes;

internal sealed class Archetype<T1, T2> (ushort id): IArchetype
    where T1 : struct, IActorComponent
    where T2 : struct, IActorComponent
{
    public ushort Id { get; } = id;
    
    public ArchetypeSign Sign { get; } = new([ActorComponentType<T1>.Id, ActorComponentType<T2>.Id]);

    private readonly Dictionary<uint, Components> _components = new();

    public bool AddFrom(IArchetype source, uint actorId)
    {
        ref var components = ref CollectionsMarshal.GetValueRefOrAddDefault(_components, actorId, out var exists);

        if (exists) // already exists
        {
            return false;
        }

        ref var existsComponent1 = ref source.TryGetRef<T1>(actorId);
        if (!Unsafe.IsNullRef(ref existsComponent1))
        {
            components.Item1 = existsComponent1;
        }

        ref var existsComponent2 = ref source.TryGetRef<T2>(actorId);
        if (!Unsafe.IsNullRef(ref existsComponent2))
        {
            components.Item2 = existsComponent2;
        }

        return true;
    }

    public bool Contains(uint actorId)
    {
        return _components.ContainsKey(actorId);
    }

    public bool Remove(uint actorId)
    {
        return _components.Remove(actorId);
    }

    public bool TryGet<T>(uint actorId, out T component) where T : struct, IActorComponent
    {
        ref var components = ref CollectionsMarshal.GetValueRefOrNullRef(_components, actorId);
        if (!Unsafe.IsNullRef(ref components))
        {
            if (typeof(T) == typeof(T1))
            {
                component = Unsafe.As<T1, T>(ref components.Item1);
                return true;
            }

            if (typeof(T) == typeof(T2))
            {
                component = Unsafe.As<T2, T>(ref components.Item2);
                return true;
            }
        }

        component = default;
        return false;
    }

    public ref T TryGetRef<T>(uint actorId) where T : struct, IActorComponent
    {
        ref var components = ref CollectionsMarshal.GetValueRefOrNullRef(_components, actorId);
        if (!Unsafe.IsNullRef(ref components))
        {
            if (typeof(T) == typeof(T1))
            {
                return ref Unsafe.As<T1, T>(ref components.Item1);
            }

            if (typeof(T) == typeof(T2))
            {
                return ref Unsafe.As<T2, T>(ref components.Item2);
            }
        }

        return ref Unsafe.NullRef<T>();
    }

    public bool Set<T>(uint actorId, ref readonly T component) where T : struct, IActorComponent
    {
        ref var components = ref CollectionsMarshal.GetValueRefOrNullRef(_components, actorId);
        if (!Unsafe.IsNullRef(ref components))
        {
            if (typeof(T) == typeof(T1))
            {
                components.Item1 = Unsafe.As<T, T1>(ref Unsafe.AsRef(in component));
                return true;
            }
            
            if (typeof(T) == typeof(T2))
            {
                components.Item2 = Unsafe.As<T, T2>(ref Unsafe.AsRef(in component));
                return true;
            }
        }

        return false;
    }

    private struct Components
    {
        public T1 Item1;
        public T2 Item2;
    }
}