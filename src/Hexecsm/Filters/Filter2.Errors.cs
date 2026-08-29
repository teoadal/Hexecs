namespace Hexecsm.Filters;

public sealed partial class Filter<T1, T2>
{
    [StackTraceHidden]
    [DoesNotReturn]
    private static void ThrowAlreadyExists(ActorId actorId)
    {
        throw new Exception($"Actor '{actorId.Value}' exists in filter");
    }
}
