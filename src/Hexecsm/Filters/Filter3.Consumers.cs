using Hexecsm.Components.Messages;
using Hexecsm.Events;
using Hexecsm.Worlds.Messages;

namespace Hexecsm.Filters;

public sealed partial class Filter<T1, T2, T3>
{
    private readonly ConsumerDelegate<ComponentAdded<T1>> _component1AddedConsumer;
    private readonly ConsumerDelegate<ComponentRemoved<T1>> _component1AddedRemovedConsumer;
    private readonly ConsumerDelegate<ComponentAdded<T2>> _component2AddedConsumer;
    private readonly ConsumerDelegate<ComponentRemoved<T2>> _component2AddedRemovedConsumer;
    private readonly ConsumerDelegate<ComponentAdded<T3>> _component3AddedConsumer;
    private readonly ConsumerDelegate<ComponentRemoved<T3>> _component3AddedRemovedConsumer;
    private readonly ConsumerDelegate<WorldClearing> _worldClearingConsumer;

    private void Handle(in ComponentAdded<T1> message)
    {
        AddedHandler1(message.ActorId);
    }

    private void Handle(in ComponentRemoved<T1> message)
    {
        RemovedHandler(message.ActorId);
    }

    private void Handle(in ComponentAdded<T2> message)
    {
        AddedHandler2(message.ActorId);
    }

    private void Handle(in ComponentRemoved<T2> message)
    {
        RemovedHandler(message.ActorId);
    }

    private void Handle(in ComponentAdded<T3> message)
    {
        AddedHandler3(message.ActorId);
    }

    private void Handle(in ComponentRemoved<T3> message)
    {
        RemovedHandler(message.ActorId);
    }

    private void Handle(in WorldClearing _)
    {
        ClearingHandler();
    }
}
