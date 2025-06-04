namespace Hexecs.Pipelines.Messages;

internal sealed class MessageQueueGroup(string group, IMessageQueue[] queues) : IMessageQueue
{
    public string Group { get; } = group;

    public void Execute()
    {
        foreach (var queue in queues)
        {
            queue.Execute();
        }
    }
}