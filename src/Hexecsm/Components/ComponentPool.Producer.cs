namespace Hexecsm.Components;

internal sealed partial class ComponentPool<T>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProduceAddedEvent(ActorId actorId, in T component)
    {
        foreach (IComponentAddedListener listener in _addedListeners)
        {
            listener.OnAdded(actorId, ComponentTypeId);
        }

        foreach (IComponentAddedListener<T> listener in _addedTypedListeners)
        {
            listener.OnAdded(actorId, in component);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProduceClearingEvent()
    {
        foreach (IComponentClearingListener listener in _clearingListeners)
        {
            listener.OnClearing();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProduceRemovedEvent(ActorId actorId, in T removed)
    {
        foreach (IComponentRemovedListener listener in _removedListeners)
        {
            listener.OnRemoved(actorId, ComponentTypeId);
        }

        foreach (IComponentRemovedListener<T> listener in _removedTypedListeners)
        {
            listener.OnRemoved(actorId, in removed);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProduceUpdatingEvent(ActorId actorId, in T exists, in T expected)
    {
        foreach (IComponentUpdatingListener<T> listener in _updatingListeners)
        {
            listener.OnUpdating(
                actorId: actorId,
                exists: in exists,
                expected: in expected);
        }
    }
}
