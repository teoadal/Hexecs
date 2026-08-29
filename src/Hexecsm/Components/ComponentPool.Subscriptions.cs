namespace Hexecsm.Components;

internal sealed partial class ComponentPool<T>
{
    private readonly List<IComponentAddedListener> _addedListeners = [];
    private readonly List<IComponentAddedListener<T>> _addedTypedListeners = [];
    private readonly List<IComponentClearingListener> _clearingListeners = [];
    private readonly List<IComponentRemovedListener> _removedListeners = [];
    private readonly List<IComponentRemovedListener<T>> _removedTypedListeners = [];
    private readonly List<IComponentUpdatingListener<T>> _updatingListeners = [];

    public void SubscribeAdded(IComponentAddedListener listener)
    {
        _addedListeners.Add(listener);
    }

    public void SubscribeAdded(IComponentAddedListener<T> listener)
    {
        _addedTypedListeners.Add(listener);
    }

    public void SubscribeClearing(IComponentClearingListener listener)
    {
        _clearingListeners.Add(listener);
    }

    public void SubscribeRemoved(IComponentRemovedListener listener)
    {
        _removedListeners.Add(listener);
    }

    public void SubscribeRemoved(IComponentRemovedListener<T> listener)
    {
        _removedTypedListeners.Add(listener);
    }

    public void SubscribeUpdating(IComponentUpdatingListener<T> listener)
    {
        _updatingListeners.Add(listener);
    }

    public void UnsubscribeAdded(IComponentAddedListener listener)
    {
        _addedListeners.Remove(listener);
    }

    public void UnsubscribeAdded(IComponentAddedListener<T> listener)
    {
        _addedTypedListeners.Remove(listener);
    }

    public void UnsubscribeClearing(IComponentClearingListener listener)
    {
        _clearingListeners.Remove(listener);
    }

    public void UnsubscribeRemoved(IComponentRemovedListener listener)
    {
        _removedListeners.Remove(listener);
    }

    public void UnsubscribeRemoved(IComponentRemovedListener<T> listener)
    {
        _removedTypedListeners.Remove(listener);
    }

    public void UnsubscribeUpdating(IComponentUpdatingListener<T> listener)
    {
        _updatingListeners.Remove(listener);
    }
}
