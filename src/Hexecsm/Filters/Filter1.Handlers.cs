namespace Hexecsm.Filters;

public sealed partial class Filter<T1>
{
    [MethodImpl(MethodImplOptions.NoInlining)]
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

    private void InitHandler()
    {
        ReadOnlySpan<ActorId> actors = _componentPool1.GetKeys().AsReadOnlySpan();

        foreach (ActorId actorId in actors)
        {
            _hashSet.TryAdd(actorId);
        }
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
