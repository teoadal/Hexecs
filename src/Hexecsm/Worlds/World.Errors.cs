using Hexecsm.Components;

namespace Hexecsm.Worlds;

public sealed partial class World
{
    [StackTraceHidden]
    [DoesNotReturn]
    private static void ThrowAlreadyExists(ActorId actorId)
    {
        throw new Exception($"Actor '{actorId.Value}' already exists in the world");
    }

    [DoesNotReturn]
    [StackTraceHidden]
    private static void ThrowActorNotFound(ActorId actorId)
    {
        throw new Exception($"Actor '{actorId.Value}' isn't found in the world");
    }

    [DoesNotReturn]
    [StackTraceHidden]
    private static void ThrowComponentAlreadyExists(ActorId actorId, ComponentTypeId componentTypeId)
    {
        Type componentType = ComponentType.GetType(componentTypeId);

        throw new Exception($"Actor '{actorId.Value}' already has component '{componentType.Name}' in the world");
    }

    [DoesNotReturn]
    [StackTraceHidden]
    private static void ThrowComponentNotFound(ActorId actorId, ComponentTypeId componentTypeId)
    {
        Type componentType = ComponentType.GetType(componentTypeId);

        throw new Exception($"Actor '{actorId.Value}' isn't have component '{componentType.Name}' in the world");
    }

    [DoesNotReturn]
    [StackTraceHidden]
    private static void ThrowComponentNotFound<T>(ActorId actorId)
    {
        throw new Exception($"Actor '{actorId.Value}' isn't have component '{typeof(T).Name}' in the world");
    }
}
