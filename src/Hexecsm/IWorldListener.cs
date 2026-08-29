namespace Hexecsm;

public interface IWorldAddedListener
{
    void OnAdded(ActorId actorId);
}

public interface IWorldClearingListener
{
    void OnClearing();
}

public interface IWorldRemovedListener
{
    void OnRemoved(ActorId actorId);
}
