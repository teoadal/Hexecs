namespace Hexecsm.Filters;

public sealed partial class Filter<T1, T2, T3>
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AddedHandler1(ActorId actorId)
    {
        if (_componentPool2.Contains(actorId) && _componentPool3.Contains(actorId))
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
        if (_componentPool1.Contains(actorId) && _componentPool3.Contains(actorId))
        {
            if (_hashSet.TryAdd(actorId))
            {
                return;
            }

            ThrowAlreadyExists(actorId);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AddedHandler3(ActorId actorId)
    {
        if (_componentPool1.Contains(actorId) && _componentPool2.Contains(actorId))
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RemovedHandler(ActorId actorId)
    {
        // Don't check remove method result.
        // Is can be call many times, because there are 3 subscriptions

        _hashSet.Remove(actorId);
    }
}
