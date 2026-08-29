using Hexecsm.Components.Messages;
using Hexecsm.Events;

namespace Hexecsm.Components;

internal sealed partial class ComponentPool<T>
{
    private readonly IProducer<ComponentAdded<T>> _addedProducer = eventBus.GetProducer<ComponentAdded<T>>();
    private readonly IProducer<ComponentAdded> _addedSimpleProducer = eventBus.GetProducer<ComponentAdded>();

    private readonly IProducer<ComponentRemoved<T>> _removedProducer = eventBus.GetProducer<ComponentRemoved<T>>();
    private readonly IProducer<ComponentRemoved> _removedSimpleProducer = eventBus.GetProducer<ComponentRemoved>();

    private readonly IProducer<ComponentUpdating<T>> _updatingProducer = eventBus.GetProducer<ComponentUpdating<T>>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProduceAddedEvent(ActorId actorId, in T added)
    {
        var message = new ComponentAdded<T>(actorId, in added);

        _addedProducer.Produce(message);
        _addedSimpleProducer.Produce(message);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProduceRemovedEvent(ActorId actorId, in T removed)
    {
        var message = new ComponentRemoved<T>(actorId, in removed);

        _removedProducer.Produce(message);
        _removedSimpleProducer.Produce(message);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProduceUpdatingEvent(ActorId actorId, in T exists, in T expected)
    {
        var message = new ComponentUpdating<T>(
            actorId: actorId,
            exists: in exists,
            expected: in expected);

        _updatingProducer.Produce(message);
    }
}
