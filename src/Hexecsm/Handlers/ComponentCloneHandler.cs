namespace Hexecsm.Handlers;

public delegate T ComponentCloneHandler<T>(ActorId source, ActorId target, in T component)
    where T : struct, IComponent;
