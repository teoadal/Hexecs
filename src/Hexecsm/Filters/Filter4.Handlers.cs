namespace Hexecsm.Filters;

public sealed partial class Filter<T1, T2, T3, T4>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddedHandler1(ActorId actorId)
    {
        if (_componentPool2.Contains(actorId) && _componentPool3.Contains(actorId) && _componentPool4.Contains(actorId))
        {
            if (_hashSet.TryAdd(actorId))
            {
                return;
            }

            ThrowAlreadyExists(actorId);
        }
    }

    private void AddedHandler<T>(ActorId actorId)
        where T : struct, IComponent
    {
        if (typeof(T) == typeof(T2))
        {
            if (_componentPool1.Contains(actorId) && _componentPool3.Contains(actorId) && _componentPool4.Contains(actorId))
            {
                if (_hashSet.TryAdd(actorId))
                {
                    return;
                }

                ThrowAlreadyExists(actorId);
            }
        }
        else if (typeof(T) == typeof(T3))
        {
            if (_componentPool1.Contains(actorId) && _componentPool2.Contains(actorId) && _componentPool4.Contains(actorId))
            {
                if (_hashSet.TryAdd(actorId))
                {
                    return;
                }

                ThrowAlreadyExists(actorId);
            }
        }
        else if (typeof(T) == typeof(T4))
        {
            if (_componentPool1.Contains(actorId) && _componentPool2.Contains(actorId) && _componentPool3.Contains(actorId))
            {
                if (_hashSet.TryAdd(actorId))
                {
                    return;
                }

                ThrowAlreadyExists(actorId);
            }
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
