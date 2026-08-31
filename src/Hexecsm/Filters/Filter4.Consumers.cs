using Hexecsm.Components.Messages;
using Hexecsm.Events;
using Hexecsm.Worlds.Messages;

namespace Hexecsm.Filters;

public sealed partial class Filter<T1, T2, T3, T4>
{
    private readonly Consumer<T2> _consumer2;
    private readonly Consumer<T3> _consumer3;
    private readonly Consumer<T4> _consumer4;

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

    private sealed class Consumer<T> : IConsumer<ComponentAdded<T>>, IConsumer<ComponentRemoved<T>>, IDisposable
        where T : struct, IComponent
    {
        private readonly EventBus _eventBus;
        private readonly Filter<T1, T2, T3, T4> _filter;

        public Consumer(EventBus eventBus, Filter<T1, T2, T3, T4> filter)
        {
            _eventBus = eventBus;
            _filter = filter;

            _eventBus.Subscribe<ComponentAdded<T>>(this);
            _eventBus.Subscribe<ComponentRemoved<T>>(this);
        }

        public void Dispose()
        {
            _eventBus.Unsubscribe<ComponentAdded<T>>(this);
            _eventBus.Unsubscribe<ComponentRemoved<T>>(this);
        }

        public void Handle(ComponentAdded<T> message)
        {
            _filter.AddedHandler<T>(message.ActorId);
        }

        public void Handle(ComponentRemoved<T> message)
        {
            _filter.RemovedHandler(message.ActorId);
        }
    }
}
