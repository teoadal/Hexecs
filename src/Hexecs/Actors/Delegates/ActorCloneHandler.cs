namespace Hexecs.Actors.Delegates;

public delegate T ActorCloneHandler<T>(in T source) where T: struct, IActorComponent;