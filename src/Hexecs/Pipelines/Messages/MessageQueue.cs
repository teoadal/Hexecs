using Hexecs.Pipelines.Attributes;

namespace Hexecs.Pipelines.Messages;

internal sealed class MessageQueue<TMessage> : IMessageQueue<TMessage>
    where TMessage : struct, IMessage
{
    public string Group { get; }

    private readonly IMessageHandler<TMessage> _handler;
    private readonly Queue<TMessage> _queue;
#if NET9_0_OR_GREATER
    private readonly Lock _lock = new Lock();
#else
    private readonly object _lock = new object();
#endif

    public MessageQueue(IMessageHandler<TMessage> handler)
    {
        Group = QueueAttribute.TryGetName(typeof(TMessage), out string name)
            ? name
            : "Default";

        _handler = handler;
        _queue = new Queue<TMessage>(16);
    }

    public void Enqueue(in TMessage message)
    {
#if NET9_0_OR_GREATER
        using (_lock.EnterScope())
#else
        lock (_lock)
#endif
        {
            _queue.Enqueue(message);
        }
    }

    public void Execute()
    {
#if NET9_0_OR_GREATER
        using (_lock.EnterScope())
#else
        lock (_lock)
#endif
        {
            while (_queue.TryDequeue(out TMessage message))
            {
                _handler.Handle(in message);
            }
        }
    }

    public bool TryEnqueue(in TMessage message)
    {
#if NET9_0_OR_GREATER
        bool result = _lock.TryEnter();
#else
        bool result = Monitor.TryEnter(_lock);
#endif
        if (!result)
        {
            return false;
        }

        try
        {
            _queue.Enqueue(message);
            return true;
        }
        finally
        {
#if NET9_0_OR_GREATER
            _lock.Exit();
#else
                Monitor.Exit(_lock);
#endif
        }
    }
}