namespace Hexecs.Pipelines.Notifications;

internal sealed class SimpleNotificationPipeline<TNotification> : INotificationHandler<TNotification>
    where TNotification : struct, INotification
{
    private readonly INotificationHandler<TNotification> _handler;

    public SimpleNotificationPipeline(INotificationHandler<TNotification> handler)
    {
        _handler = handler;
    }

    public void Handle(in TNotification notification) => _handler.Handle(in notification);
}