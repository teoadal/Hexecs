namespace Hexecsm.Filters;

public sealed partial class Filter<T1>
{
    [StackTraceHidden]
    [DoesNotReturn]
    private static void ThrowAlreadyExists(ActorId actorId)
    {
        throw new Exception($"Actor '{actorId.Value}' exists in filter");
    }

    [StackTraceHidden]
    [DoesNotReturn]
    private static void ThrowNotFound(ActorId actorId)
    {
        throw new Exception($"Actor '{actorId.Value}' isn't found in filter");
    }
}
