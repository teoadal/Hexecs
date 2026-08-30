namespace Hexecsm.Filters;

public sealed partial class Filter<T1, T2, T3>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        if (_hashSet.Remove(actorId))
        {
            return;
        }

        ThrowNotFound(actorId);
    }
}
