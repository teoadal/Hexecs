namespace Hexecs.Pipelines.Notifications;

internal sealed class NotificationPipeline<TNotification> : INotificationHandler<TNotification>
    where TNotification : struct, INotification
{
    private readonly INotificationHandler<TNotification>[] _handlers;

    public NotificationPipeline(INotificationHandler<TNotification>[] handlers)
    {
        _handlers = handlers;
    }

    public void Handle(in TNotification notification)
    {
        foreach (var handler in _handlers)
        {
            handler.Handle(in notification);
        }
    }
}