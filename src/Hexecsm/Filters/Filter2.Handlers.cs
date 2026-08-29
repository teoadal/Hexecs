namespace Hexecsm.Filters;

public sealed partial class Filter<T1, T2>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddedHandler1(ActorId actorId)
    {
        if (_componentPool2.Contains(actorId))
        {
            _hashSet.TryAdd(actorId);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddedHandler2(ActorId actorId)
    {
        if (_componentPool1.Contains(actorId))
        {
            _hashSet.TryAdd(actorId);
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
        _hashSet.Remove(actorId);
    }
}
