using Hexecs.Pipelines.Attributes;

namespace Hexecs.Pipelines.Messages;

internal sealed class MessageQueue<TMessage> : IMessageQueue<TMessage>
    where TMessage : struct, IMessage
{
    public string Group { get; }

    private readonly IMessageHandler<TMessage> _handler;
    private readonly Queue<TMessage> _queue;
    private readonly Lock _lock;

    public MessageQueue(IMessageHandler<TMessage> handler)
    {
        Group = QueueAttribute.TryGetName(typeof(TMessage), out var name)
            ? name
            : "Default";

        _handler = handler;
        _queue = new Queue<TMessage>(16);
        _lock = new Lock();
    }

    public void Enqueue(in TMessage message)
    {
        using var locker = _lock.EnterScope();

        _queue.Enqueue(message);
    }

    public void Execute()
    {
        using var locker = _lock.EnterScope();

        while (_queue.TryDequeue(out var message))
        {
            _handler.Handle(in message);
        }
    }

    public bool TryEnqueue(in TMessage message)
    {
        var result = _lock.TryEnter();
        if (!result) return false;

        try
        {
            _queue.Enqueue(message);
            return true;
        }
        finally
        {
            _lock.Exit();
        }
    }
}