namespace Hexecsm.Filters;

public sealed partial class Filter<T1, T2>
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AddedHandler1(ActorId actorId)
    {
        if (_componentPool2.Contains(actorId))
        {
            if (_hashSet.TryAdd(actorId))
            {
                return;
            }

            ThrowAlreadyExists(actorId);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AddedHandler2(ActorId actorId)
    {
        if (_componentPool1.Contains(actorId))
        {
            if (_hashSet.TryAdd(actorId))
            {
                return;
            }

            ThrowAlreadyExists(actorId);
        }
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
            if (_componentPool2.Contains(actorId))
            {
                _hashSet.TryAdd(actorId);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RemovedHandler(ActorId actorId)
    {
        // Don't check remove method result.
        // Is can be call many times, because there are 2 subscriptions

        _hashSet.Remove(actorId);
    }
}
