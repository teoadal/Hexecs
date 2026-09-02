using Hexecsm.Components.Messages;
using Hexecsm.Events;

namespace Hexecsm.Components;

internal sealed partial class ComponentPool<T>
{
    private readonly IProducer<ComponentAdded<T>> _addedProducer;
    private readonly IProducer<ComponentRemoved<T>> _removedProducer;
    private readonly IProducer<ComponentUpdating<T>> _updatingProducer;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProduceAddedEvent(ActorId actorId, in T added)
    {
        var message = new ComponentAdded<T>(actorId, in added);

        _addedProducer.Produce(in message);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProduceRemovedEvent(ActorId actorId, in T removed)
    {
        var message = new ComponentRemoved<T>(actorId, in removed);

        _removedProducer.Produce(in message);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProduceUpdatingEvent(ActorId actorId, in T exists, in T expected)
    {
        var message = new ComponentUpdating<T>(
            actorId: actorId,
            exists: in exists,
            expected: in expected);

        _updatingProducer.Produce(in message);
    }
}
