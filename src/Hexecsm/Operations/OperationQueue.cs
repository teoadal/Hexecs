using Hexecsm.Components;
using Hexecsm.Utils;
using Hexecsm.Worlds;

namespace Hexecsm.Operations;

internal sealed partial class OperationQueue
{
    private readonly ComponentBufferManager _componentBufferManager;

    public OperationQueue(
        ComponentPoolManager componentPools,
        World world)
    {
        _componentBufferManager = new ComponentBufferManager(componentPools);
    }

    public void ActorAdd(ActorId actorId)
    {
        var operation = new Operation(OperationType.ActorAdd, actorId);

        Enqueue(in operation);
    }

    public void ActorRemove(ActorId actorId)
    {
        var operation = new Operation(OperationType.ActorRemove, actorId);

        Enqueue(in operation);
    }

    public void ComponentAdd<T>(ActorId actorId, in T component)
        where T : struct, IComponent
    {
        ComponentBuffer<T> buffer = _componentBufferManager.GetOrAdd<T>();
        int componentIndex = buffer.Add(in component);

        var operation = new Operation(OperationType.ComponentAdd, actorId, ComponentType<T>.Id, componentIndex);

        Enqueue(in operation);
    }

    public void ComponentClone<T>(ActorId from, ActorId target)
        where T : struct, IComponent
    {
        ComponentBuffer<T> buffer = _componentBufferManager.GetOrAdd<T>();
        ref T component = ref buffer.Pool.GetRef(from);
        int componentIndex = buffer.Add(in component);

        var operation = new Operation(OperationType.ComponentClone, target, ComponentType<T>.Id, componentIndex);

        Enqueue(in operation);
    }

    public void ComponentRemove<T>(ActorId actorId)
        where T : struct, IComponent
    {
        var operation = new Operation(OperationType.ComponentRemove, actorId, ComponentType<T>.Id);

        Enqueue(in operation);
    }

    public void ComponentUpdate<T>(ActorId actorId, in T component)
        where T : struct, IComponent
    {
        ComponentBuffer<T> buffer = _componentBufferManager.GetOrAdd<T>();
        int componentIndex = buffer.Add(in component);

        var operation = new Operation(OperationType.ComponentUpdate, actorId, ComponentType<T>.Id, componentIndex);

        Enqueue(in operation);
    }
}
