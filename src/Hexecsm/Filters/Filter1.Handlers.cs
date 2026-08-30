namespace Hexecsm.Filters;

public sealed partial class Filter<T1>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddedHandler1(ActorId actorId)
    {
        if (_hashSet.TryAdd(actorId))
        {
            return;
        }

        ThrowAlreadyExists(actorId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ClearingHandler()
    {
        _hashSet.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RemovedHandler(ActorId actorId)
    {
        if (_hashSet.Remove(actorId))
        {
            return;
        }

        ThrowNotFound(actorId);
    }
}
