namespace Hexecsm;

public sealed partial class World
{
    [DoesNotReturn]
    [StackTraceHidden]
    private static void ThrowActorNotFound(ActorId actorId)
    {
        throw new Exception($"Actor '{actorId.Value}' isn't found");
    }

    [DoesNotReturn]
    [StackTraceHidden]
    private static void ThrowComponentNotFound<T>(ActorId actorId)
    {
        throw new Exception($"Actor '{actorId.Value}' isn't have component '{nameof(T)}'");
    }
}
