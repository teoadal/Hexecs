namespace Hexecsm.Events;

internal sealed partial class EventBus
{
    private sealed class Producer<TMessage> : IProducer<TMessage>
        where TMessage : struct, IMessage
    {
        private readonly List<ConsumerDelegate<TMessage>> _consumers = [];
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

        [SkipLocalsInit]
        public void Produce(in TMessage message)
        {
            ObjectDisposedException.ThrowIf(_disposed, typeof(Producer<TMessage>));

            Span<ConsumerDelegate<TMessage>> consumers = CollectionsMarshal.AsSpan(_consumers);

            foreach (ConsumerDelegate<TMessage> consumer in consumers)
            {
                consumer(in message);
            }
        }

        [SkipLocalsInit]
        public void Produce(ReadOnlySpan<TMessage> messages)
        {
            ObjectDisposedException.ThrowIf(_disposed, typeof(Producer<TMessage>));

            Span<ConsumerDelegate<TMessage>> consumers = CollectionsMarshal.AsSpan(_consumers);

            foreach (ConsumerDelegate<TMessage> consumer in consumers)
            {
                foreach (ref readonly TMessage message in messages)
                {
                    consumer(in message);
                }
            }
        }

        public void Subscribe(ConsumerDelegate<TMessage> consumer)
        {
            ObjectDisposedException.ThrowIf(_disposed, typeof(Producer<TMessage>));

            using (_lock.EnterScope())
            {
                if (!_consumers.Contains(consumer))
                {
                    _consumers.Add(consumer);

                    return;
                }

                ThrowConsumerAlreadyRegistered();
            }
        }

        public void Unsubscribe(ConsumerDelegate<TMessage> consumer)
        {
            ObjectDisposedException.ThrowIf(_disposed, typeof(Producer<TMessage>));

            using (_lock.EnterScope())
            {
                if (_consumers.Remove(consumer))
                {
                    return;
                }

                ThrowConsumerNotRegistered();
            }
        }
    }
}
