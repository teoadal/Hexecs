namespace Hexecs.Actors.Delegates;

public delegate bool ActorPredicate<T1>(in ActorRef<T1> component)
    where T1 : struct, IActorComponent;

public delegate bool ActorPredicate<T1, T2>(in ActorRef<T1, T2> component)
    where T1 : struct, IActorComponent
    where T2 : struct, IActorComponent;
    
public delegate bool ActorPredicate<T1, T2, T3>(in ActorRef<T1, T2, T3> component)
    where T1 : struct, IActorComponent
    where T2 : struct, IActorComponent
    where T3 : struct, IActorComponent;