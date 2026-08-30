using Hexecsm.Components.Messages;
using Hexecsm.Events;
using Hexecsm.Worlds.Messages;

namespace Hexecsm.Filters;

public sealed partial class Filter<T1, T2, T3>
{
    private readonly Consumer2 _consumer2;
    private readonly Consumer3 _consumer3;

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

    private sealed class Consumer2
        : IConsumer<ComponentAdded<T2>>, IConsumer<ComponentRemoved<T2>>, IDisposable
    {
        private readonly EventBus _eventBus;
        private readonly Filter<T1, T2, T3> _filter;

        public Consumer2(EventBus eventBus, Filter<T1, T2, T3> filter)
        {
            _eventBus = eventBus;
            _filter = filter;

            _eventBus.Subscribe<ComponentAdded<T2>>(this);
            _eventBus.Subscribe<ComponentRemoved<T2>>(this);
        }

        public void Dispose()
        {
            _eventBus.Unsubscribe<ComponentAdded<T2>>(this);
            _eventBus.Unsubscribe<ComponentRemoved<T2>>(this);
        }

        public void Handle(ComponentAdded<T2> message)
        {
            _filter.AddedHandler2(message.ActorId);
        }

        public void Handle(ComponentRemoved<T2> message)
        {
            _filter.RemovedHandler(message.ActorId);
        }
    }

    private sealed class Consumer3
        : IConsumer<ComponentAdded<T3>>, IConsumer<ComponentRemoved<T3>>, IDisposable
    {
        private readonly EventBus _eventBus;
        private readonly Filter<T1, T2, T3> _filter;

        public Consumer3(EventBus eventBus, Filter<T1, T2, T3> filter)
        {
            _eventBus = eventBus;
            _filter = filter;

            _eventBus.Subscribe<ComponentAdded<T3>>(this);
            _eventBus.Subscribe<ComponentRemoved<T3>>(this);
        }

        public void Dispose()
        {
            _eventBus.Unsubscribe<ComponentAdded<T3>>(this);
            _eventBus.Unsubscribe<ComponentRemoved<T3>>(this);
        }

        public void Handle(ComponentAdded<T3> message)
        {
            _filter.AddedHandler3(message.ActorId);
        }

        public void Handle(ComponentRemoved<T3> message)
        {
            _filter.RemovedHandler(message.ActorId);
        }
    }
}
