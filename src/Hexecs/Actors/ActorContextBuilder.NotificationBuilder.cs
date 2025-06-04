using Hexecs.Pipelines;
using Hexecs.Pipelines.Notifications;

namespace Hexecs.Actors;

public sealed partial class ActorContextBuilder
{
    public sealed class NotificationBuilder<T>
        where T : struct, INotification
    {
        private readonly List<Entry<INotificationHandler<T>>> _handlers = [];

        public NotificationBuilder<T> Add(INotificationHandler<T> handler)
        {
            _handlers.Add(new Entry<INotificationHandler<T>>(handler));
            return this;
        }

        public NotificationBuilder<T> Create(Func<ActorContext, INotificationHandler<T>> handler)
        {
            _handlers.Add(new Entry<INotificationHandler<T>>(handler));
            return this;
        }

        internal Func<ActorContext, INotificationHandler> Build() => actorContext =>
        {
            INotificationHandler result = _handlers.Count == 1
                ? new SimpleNotificationPipeline<T>(_handlers[0].Invoke(actorContext))
                : new NotificationPipeline<T>(_handlers
                    .Select((entry, ctx) => entry.Invoke(ctx), actorContext)
                    .Order(OrderComparer<INotificationHandler<T>>.CreateInstance())
                    .ToArray());

            _handlers.Clear();

            return result;
        };
    }
}