namespace Hexecsm.Handlers;

public delegate void ComponentDisposeHandler<T>(ActorId actorId, in T component)
    where T : struct, IComponent;
