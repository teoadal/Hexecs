namespace Hexecsm.Filters;

public sealed partial class Filter<T1, T2, T3, T4>
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
