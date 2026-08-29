namespace Hexecsm;

public sealed partial class World
{
    private readonly List<IWorldAddedListener> _addedListeners = [];
    private readonly List<IWorldClearingListener> _clearingListeners = [];
    private readonly List<IWorldRemovedListener> _removedListeners = [];

    public void RaiseAddedEvent(ActorId actorId)
    {
        foreach (IWorldAddedListener listener in _addedListeners)
        {
            listener.OnAdded(actorId);
        }
    }

    public void RaiseClearingEvent()
    {
        foreach (IWorldClearingListener clearingListener in _clearingListeners)
        {
            clearingListener.OnClearing();
        }
    }

    public void RaiseRemovedEvent(ActorId actorId)
    {
        foreach (IWorldRemovedListener listener in _removedListeners)
        {
            listener.OnRemoved(actorId);
        }
    }

    public void SubscribeAdded(IWorldAddedListener listener)
    {
        _addedListeners.Add(listener);
    }

    public void SubscribeClearing(IWorldClearingListener listener)
    {
        _clearingListeners.Add(listener);
    }

    public void SubscribeRemoved(IWorldRemovedListener listener)
    {
        _removedListeners.Add(listener);
    }

    public void UnsubscribeAdded(IWorldAddedListener listener)
    {
        _addedListeners.Remove(listener);
    }

    public void UnsubscribeClearing(IWorldClearingListener listener)
    {
        _clearingListeners.Remove(listener);
    }

    public void UnsubscribeRemoved(IWorldRemovedListener listener)
    {
        _removedListeners.Remove(listener);
    }
}
