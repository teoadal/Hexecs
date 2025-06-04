namespace Hexecs.Actors.Delegates;

public delegate void ActorDisposeHandler<T>(ref T component) where T : struct, IActorComponent;