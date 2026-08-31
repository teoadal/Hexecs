namespace Hexecsm.Events;

internal sealed class EventBus : IDisposable
{
    private readonly Dictionary<Type, IProducer> _producers = [];
    private readonly Lock _producerLock = new Lock();

    public void Dispose()
    {
        foreach (IProducer producer in _producers.Values)
        {
            producer.Dispose();
        }

        _producers.Clear();
    }

    public IProducer<TMessage> GetProducer<TMessage>()
        where TMessage : struct, IMessage
    {
        return GetOrCreateProducer<TMessage>();
    }

    public void Subscribe<TMessage>(IConsumer<TMessage> consumer)
        where TMessage : struct, IMessage
    {
        Producer<TMessage> producer = GetOrCreateProducer<TMessage>();
        producer.Subscribe(consumer);
    }

    public void Unsubscribe<TMessage>(IConsumer<TMessage> consumer)
        where TMessage : struct, IMessage
    {
        Producer<TMessage> producer = GetOrCreateProducer<TMessage>();
        producer.Unsubscribe(consumer);
    }

    private Producer<TMessage> GetOrCreateProducer<TMessage>()
        where TMessage : struct, IMessage
    {
        Type type = typeof(TMessage);

        using (_producerLock.EnterScope())
        {
            if (_producers.TryGetValue(type, out IProducer? existsProducer))
            {
                return (Producer<TMessage>)existsProducer;
            }

            var producer = new Producer<TMessage>();

            _producers[type] = producer;

            return producer;
        }
    }

    private sealed class Producer<TMessage> : IProducer<TMessage>
        where TMessage : struct, IMessage
    {
        private readonly List<IConsumer<TMessage>> _consumers = [];
        private readonly Lock _lock = new Lock();

        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            _consumers.Clear();
        }

        public void Produce(TMessage message)
        {
            ObjectDisposedException.ThrowIf(_disposed, typeof(Producer<TMessage>));

            Span<IConsumer<TMessage>> consumers = CollectionsMarshal.AsSpan(_consumers);

            foreach (IConsumer<TMessage> consumer in consumers)
            {
                consumer.Handle(message);
            }
        }

        public void Subscribe(IConsumer<TMessage> consumer)
        {
            ObjectDisposedException.ThrowIf(_disposed, typeof(Producer<TMessage>));

            using (_lock.EnterScope())
            {
                _consumers.Add(consumer);
            }
        }

        public void Unsubscribe(IConsumer<TMessage> consumer)
        {
            ObjectDisposedException.ThrowIf(_disposed, typeof(Producer<TMessage>));

            using (_lock.EnterScope())
            {
                _consumers.Remove(consumer);
            }
        }
    }
}
