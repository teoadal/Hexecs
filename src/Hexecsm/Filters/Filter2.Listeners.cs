using Hexecsm.Components;

namespace Hexecsm.Filters;

public sealed partial class Filter<T1, T2>
{
    public void OnAdded(ActorId actorId, ComponentTypeId componentTypeId)
    {
        if (componentTypeId == ComponentType<T1>.Id)
        {
            AddedHandler1(actorId);
        }
        else if (componentTypeId == ComponentType<T2>.Id)
        {
            AddedHandler2(actorId);
        }
    }

    public void OnClearing()
    {
        ClearingHandler();
    }

    public void OnRemoved(ActorId actorId, ComponentTypeId componentTypeId)
    {
        RemovedHandler(actorId);
    }
}
