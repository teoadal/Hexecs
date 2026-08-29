namespace Hexecsm.Components;

internal sealed partial class ComponentPool<T>
{
    [StackTraceHidden]
    [DoesNotReturn]
    private static void ThrowAlreadyExists(ActorId actorId)
    {
        throw new Exception($"Actor '{actorId.Value}' already has component '{typeof(T).Name}'");
    }

    [StackTraceHidden]
    [DoesNotReturn]
    private static void ThrowComponentNotFound(ActorId actorId)
    {
        throw new Exception($"Actor '{actorId.Value}' isn't have component '{typeof(T).Name}'");
    }

    [StackTraceHidden]
    [DoesNotReturn]
    private static void ThrowInvalidOperation(ActorId actorId, OperationType type)
    {
        throw new Exception($"Operation '{type}' for '{actorId.Value}' isn't supported");
    }
}
