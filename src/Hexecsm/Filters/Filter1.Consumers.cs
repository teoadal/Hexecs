using Hexecsm.Components.Messages;
using Hexecsm.Events;
using Hexecsm.Worlds.Messages;

namespace Hexecsm.Filters;

public sealed partial class Filter<T1>
{
    private readonly ConsumerDelegate<ComponentAdded<T1>> _component1AddedConsumer;
    private readonly ConsumerDelegate<ComponentRemoved<T1>> _component1AddedRemovedConsumer;
    private readonly ConsumerDelegate<WorldClearing> _worldClearingConsumer;

    private void Handle(in ComponentAdded<T1> message)
    {
        AddedHandler1(message.ActorId);
    }

    private void Handle(in ComponentRemoved<T1> message)
    {
        RemovedHandler(message.ActorId);
    }

    private void Handle(in WorldClearing _)
    {
        ClearingHandler();
    }
}
