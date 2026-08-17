using Hexecs.Arch.Components;

namespace Hexecs.Arch.Archetypes;

internal struct ArchetypeEntry<T1, T2> : IArchetypeEntry<ArchetypeEntry<T1, T2>>
    where T1 : struct, IActorComponent
    where T2 : struct, IActorComponent
{
    public uint Id;
    public T1 Component1;
    public T2 Component2;

    public static ArchetypeAccessor<TComponent1>
        GetAccessor<TComponent1>(ref ArchetypeEntry<T1, T2> entry)
        where TComponent1 : struct, IActorComponent
    {
        return new ArchetypeAccessor<TComponent1>(
            entry.Id,
            ref TryGetRef<TComponent1>(ref entry));
    }
    
    public static ArchetypeAccessor<TComponent1, TComponent2>
        GetAccessor<TComponent1, TComponent2>(ref ArchetypeEntry<T1, T2> entry)
        where TComponent1 : struct, IActorComponent
        where TComponent2 : struct, IActorComponent
    {
        return new ArchetypeAccessor<TComponent1, TComponent2>(
            entry.Id,
            ref TryGetRef<TComponent1>(ref entry),
            ref TryGetRef<TComponent2>(ref entry));
    }

    public static ArchetypeAccessor<TComponent1, TComponent2, TComponent3>
        GetAccessor<TComponent1, TComponent2, TComponent3>(ref ArchetypeEntry<T1, T2> entry)
        where TComponent1 : struct, IActorComponent
        where TComponent2 : struct, IActorComponent
        where TComponent3 : struct, IActorComponent
    {
        return new ArchetypeAccessor<TComponent1, TComponent2, TComponent3>(
            entry.Id,
            ref TryGetRef<TComponent1>(ref entry),
            ref TryGetRef<TComponent2>(ref entry),
            ref Unsafe.NullRef<TComponent3>());
    }

    public static uint GetId(ref ArchetypeEntry<T1, T2> components)
    {
        return components.Id;
    }

    public static bool Has<T>() where T : struct, IActorComponent
    {
        return typeof(T) == typeof(T1) ||
               typeof(T) == typeof(T2);
    }

    public static ref TComponent TryGetRef<TComponent>(ref ArchetypeEntry<T1, T2> components)
        where TComponent : struct, IActorComponent
    {
        if (typeof(TComponent) == typeof(T1))
        {
            return ref Unsafe.As<T1, TComponent>(ref components.Component1);
        }

        if (typeof(TComponent) == typeof(T2))
        {
            return ref Unsafe.As<T2, TComponent>(ref components.Component2);
        }

        return ref Unsafe.NullRef<TComponent>();
    }
}