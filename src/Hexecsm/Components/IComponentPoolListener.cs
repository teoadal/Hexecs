namespace Hexecsm.Components;

internal interface IComponentPoolListener<T>
    where T : struct, IComponent
{
    void OnAdded(ActorId actorId, in T component);

    void OnClearing();

    void OnRemoved(ActorId actorId, in T component);

    void OnUpdating(ActorId actorId, in T exists, in T expected);
}
