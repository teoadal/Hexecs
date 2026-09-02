using Hexecsm.Utils;

namespace Hexecsm.Events;

internal sealed partial class EventBus : IDisposable
{
    private readonly Dictionary<Type, IProducer> _producers = new Dictionary<Type, IProducer>(ReferenceComparer<Type>.Instance);
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

    public void Subscribe<TMessage>(ConsumerDelegate<TMessage> consumer)
        where TMessage : struct, IMessage
    {
        Producer<TMessage> producer = GetOrCreateProducer<TMessage>();
        producer.Subscribe(consumer);
    }

    public void Unsubscribe<TMessage>(ConsumerDelegate<TMessage> consumer)
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
}
