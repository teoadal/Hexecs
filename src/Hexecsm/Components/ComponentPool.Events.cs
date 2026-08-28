namespace Hexecsm.Components;

internal sealed partial class ComponentPool<T>
{
    private readonly List<IComponentPoolListener<T>> _listeners = [];

    public void Subscribe(IComponentPoolListener<T> listener)
    {
        _listeners.Add(listener);
    }

    public void Unsubscribe(IComponentPoolListener<T> listener)
    {
        _listeners.Remove(listener);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RaiseAddedEvent(ActorId actorId, in T component)
    {
        foreach (IComponentPoolListener<T> listener in _listeners)
        {
            listener.OnAdded(actorId, in component);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RaiseClearingEvent()
    {
        foreach (IComponentPoolListener<T> listener in _listeners)
        {
            listener.OnClearing();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RaiseRemovedEvent(ActorId actorId, in T removed)
    {
        foreach (IComponentPoolListener<T> listener in _listeners)
        {
            listener.OnRemoved(actorId, in removed);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RaiseUpdatingEvent(ActorId actorId, in T exists, in T expected)
    {
        foreach (IComponentPoolListener<T> listener in _listeners)
        {
            listener.OnUpdating(
                actorId: actorId,
                exists: in exists,
                expected: in expected);
        }
    }
}
