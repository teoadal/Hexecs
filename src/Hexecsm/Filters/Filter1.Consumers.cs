using Hexecsm.Components.Messages;
using Hexecsm.Events;
using Hexecsm.Worlds.Messages;

namespace Hexecsm.Filters;

public sealed partial class Filter<T1>
{
    void IConsumer<ComponentAdded<T1>>.Handle(ComponentAdded<T1> message)
    {
        AddedHandler1(message.ActorId);
    }

    void IConsumer<ComponentRemoved<T1>>.Handle(ComponentRemoved<T1> message)
    {
        RemovedHandler(message.ActorId);
    }

    void IConsumer<WorldClearing>.Handle(WorldClearing message)
    {
        ClearingHandler();
    }
}
