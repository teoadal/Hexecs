using Hexecs.Assets;
using Hexecs.Pipelines;
using Hexecs.Worlds;

namespace Hexecs.Tests.Pipelines;

public sealed class PipelineTestFixture : BaseFixture, IDisposable
{
    public ActorContext Actors => World.Actors;
    public AssetContext Assets => World.Assets;
    public readonly World World = new WorldBuilder().Build();

    public ICommandHandler<TCommand, TResult> CreateCommandHandler<TCommand, TResult>(Func<TCommand, TResult> handler)
        where TCommand : struct, ICommand<TResult>
    {
        return new CommandHandler<TCommand, TResult>(handler);
    }

    public IQueryHandler<TQuery, TResult> CreateQueryHandler<TQuery, TResult>(Func<TQuery, TResult> handler)
        where TQuery : struct, IQuery<TResult>
    {
        return new QueryHandler<TQuery, TResult>(handler);
    }

    public INotificationHandler<TNotification> CreateNotificationHandler<TNotification>(Action<TNotification> handler)
        where TNotification : struct, INotification
    {
        return new NotificationHandler<TNotification>(handler);
    }

    private sealed class CommandHandler<TCommand, TResult>(Func<TCommand, TResult> handler)
        : ICommandHandler<TCommand, TResult>
        where TCommand : struct, ICommand<TResult>
    {
        public TResult Handle(in TCommand command) => handler(command);
    }

    private sealed class QueryHandler<TQuery, TResult>(Func<TQuery, TResult> handler)
        : IQueryHandler<TQuery, TResult>
        where TQuery : struct, IQuery<TResult>
    {
        public TResult Handle(in TQuery query) => handler(query);
    }

    private sealed class NotificationHandler<TNotification>(Action<TNotification> handler)
        : INotificationHandler<TNotification>
        where TNotification : struct, INotification
    {
        public void Handle(in TNotification notification) => handler(notification);
    }

    public void Dispose()
    {
        World.Dispose();
    }
}