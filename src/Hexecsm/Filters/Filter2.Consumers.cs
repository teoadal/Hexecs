using Hexecsm.Components.Messages;
using Hexecsm.Events;
using Hexecsm.Worlds.Messages;

namespace Hexecsm.Filters;

public sealed partial class Filter<T1, T2>
{
    private readonly Consumer1 _consumer1;
    private readonly Consumer2 _consumer2;

    public void Handle(WorldClearing message)
    {
        ClearingHandler();
    }

    private sealed class Consumer1
        : IConsumer<ComponentAdded<T1>>, IConsumer<ComponentRemoved<T1>>, IDisposable
    {
        private readonly EventBus _eventBus;
        private readonly Filter<T1, T2> _filter;

        public Consumer1(EventBus eventBus, Filter<T1, T2> filter)
        {
            _eventBus = eventBus;
            _filter = filter;

            _eventBus.Subscribe<ComponentAdded<T1>>(this);
            _eventBus.Subscribe<ComponentRemoved<T1>>(this);
        }

        public void Handle(ComponentAdded<T1> message)
        {
            _filter.AddedHandler1(message.ActorId);
        }

        public void Handle(ComponentRemoved<T1> message)
        {
            _filter.RemovedHandler(message.ActorId);
        }

        public void Dispose()
        {
            _eventBus.Unsubscribe<ComponentAdded<T1>>(this);
            _eventBus.Unsubscribe<ComponentRemoved<T1>>(this);
        }
    }

    private sealed class Consumer2
        : IConsumer<ComponentAdded<T2>>, IConsumer<ComponentRemoved<T2>>, IDisposable
    {
        private readonly EventBus _eventBus;
        private readonly Filter<T1, T2> _filter;

        public Consumer2(EventBus eventBus, Filter<T1, T2> filter)
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
}
