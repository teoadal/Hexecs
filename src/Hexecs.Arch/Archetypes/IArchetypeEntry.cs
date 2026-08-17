using Hexecs.Arch.Components;

namespace Hexecs.Arch.Archetypes;

internal interface IArchetypeEntry<TSelf>
    where TSelf : struct, IArchetypeEntry<TSelf>
{
    static abstract bool Has<T>()
        where T : struct, IActorComponent;

    static abstract ArchetypeAccessor<T1> GetAccessor<T1>(ref TSelf entry)
        where T1 : struct, IActorComponent;
    
    static abstract ArchetypeAccessor<T1, T2> GetAccessor<T1, T2>(ref TSelf entry)
        where T1 : struct, IActorComponent
        where T2 : struct, IActorComponent;

    static abstract ArchetypeAccessor<T1, T2, T3> GetAccessor<T1, T2, T3>(ref TSelf entry)
        where T1 : struct, IActorComponent
        where T2 : struct, IActorComponent
        where T3 : struct, IActorComponent;

    static abstract uint GetId(ref TSelf entry);

    static abstract ref T TryGetRef<T>(ref TSelf entry)
        where T : struct, IActorComponent;
}