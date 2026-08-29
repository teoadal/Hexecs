namespace Hexecsm.Components;

internal interface IComponentAddedListener
{
    void OnAdded(ActorId actorId, ComponentTypeId componentTypeId);
}

internal interface IComponentAddedListener<T>
    where T : struct, IComponent
{
    void OnAdded(ActorId actorId, in T component);
}

internal interface IComponentClearingListener
{
    void OnClearing();
}

internal interface IComponentRemovedListener
{
    void OnRemoved(ActorId actorId, ComponentTypeId componentTypeId);
}

internal interface IComponentRemovedListener<T>
    where T : struct, IComponent
{
    void OnRemoved(ActorId actorId, in T component);
}

internal interface IComponentUpdatingListener<T>
    where T : struct, IComponent
{
    void OnUpdating(ActorId actorId, in T exists, in T expected);
}
