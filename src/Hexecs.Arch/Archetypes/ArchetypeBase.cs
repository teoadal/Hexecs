using System.Collections;
using Hexecs.Arch.Components;

namespace Hexecs.Arch.Archetypes;

internal abstract partial class ArchetypeBase<TEntry>(ushort id, int capacity, ArchetypeSign sign) : IArchetype
    where TEntry : struct, IArchetypeEntry<TEntry>
{
    public ushort Id { get; } = id;

    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _count;
    }

    public ArchetypeSign Sign { get; } = sign;

    public abstract bool AddFrom(IArchetype source, uint actorId);

    public bool Contains(uint actorId)
    {
        return ContainsEntry(actorId);
    }

    public bool Contains<T>(uint actorId) where T : struct, IActorComponent
    {
        return TEntry.Has<T>() && ContainsEntry(actorId);
    }

    public IEnumerable<ArchetypeAccessor<T1, T2>> Filter<T1, T2>()
        where T1 : struct, IActorComponent
        where T2 : struct, IActorComponent
    {
        return new AccessEnumerator<T1, T2>(this);
    }

    public IEnumerable<ArchetypeAccessor<T1, T2, T3>> Filter<T1, T2, T3>()
        where T1 : struct, IActorComponent
        where T2 : struct, IActorComponent
        where T3 : struct, IActorComponent
    {
        return new AccessEnumerator<T1, T2, T3>(this);
    }

    public bool Remove(uint actorId)
    {
        return RemoveEntry(actorId, out _);
    }

    public bool Remove<T>(uint actorId, out T component) where T : struct, IActorComponent
    {
        if (RemoveEntry(actorId, out var entry))
        {
            component = TEntry.TryGetRef<T>(ref entry);
            return true;
        }

        component = default;
        return false;
    }

    public bool TryGet<T>(uint actorId, out T component) where T : struct, IActorComponent
    {
        ref var entry = ref TryGetEntryRef(actorId);
        ref var existsComponent = ref TEntry.TryGetRef<T>(ref entry);
        if (Unsafe.IsNullRef(ref existsComponent))
        {
            component = default;
            return false;
        }

        component = existsComponent;
        return true;
    }

    public ArchetypeAccessor<T1> TryGetAccessor<T1>(uint actorId)
        where T1 : struct, IActorComponent
    {
        ref var entry = ref TryGetEntryRef(actorId);
        return TEntry.GetAccessor<T1>(ref entry);
    }

    public ArchetypeAccessor<T1, T2> TryGetAccessor<T1, T2>(uint actorId)
        where T1 : struct, IActorComponent
        where T2 : struct, IActorComponent
    {
        ref var entry = ref TryGetEntryRef(actorId);
        return TEntry.GetAccessor<T1, T2>(ref entry);
    }

    public ArchetypeAccessor<T1, T2, T3> TryGetAccessor<T1, T2, T3>(uint actorId)
        where T1 : struct, IActorComponent
        where T2 : struct, IActorComponent
        where T3 : struct, IActorComponent
    {
        ref var entry = ref TryGetEntryRef(actorId);
        return TEntry.GetAccessor<T1, T2, T3>(ref entry);
    }

    public ref T TryGetRef<T>(uint actorId)
        where T : struct, IActorComponent
    {
        ref var entry = ref TryGetEntryRef(actorId);
        return ref Unsafe.IsNullRef(ref entry)
            ? ref Unsafe.NullRef<T>()
            : ref TEntry.TryGetRef<T>(ref entry);
    }

    public bool Set<T>(uint actorId, ref readonly T component)
        where T : struct, IActorComponent
    {
        ref var components = ref TryGetEntryRef(actorId);
        if (Unsafe.IsNullRef(ref components))
        {
            return false;
        }

        ref var exists = ref TEntry.TryGetRef<T>(ref components);
        exists = component;

        return false;
    }
}