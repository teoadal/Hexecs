using Hexecs.Pipelines;
using Hexecs.Tests.Mocks;

namespace Hexecs.Tests.Pipelines;

public sealed class NotificationShould(PipelineTestFixture fixture) : IClassFixture<PipelineTestFixture>
{
    [Fact]
    public void BeHandled_IfOneHandler()
    {
        // arrange

        var handler1 = new Mock<INotificationHandler<NotificationMock>>();
        handler1.Setup(h => h.Handle(It.IsAny<NotificationMock>()));

        var context = fixture.World.CreateActorContext(ctx => ctx.AddNotificationHandler(handler1.Object));
        var notification = new NotificationMock(25);

        // act

        context
            .Invoking(ctx => ctx.Publish(notification))
            .Should()
            .NotThrow();

        // assert

        handler1.Verify(h => h.Handle(notification), Times.Once);

        fixture.World.RemoveActorContext(context);
    }

    [Fact]
    public void BeHandled_IfManyHandlers()
    {
        // arrange

        var handlers = fixture.CreateArray(_ =>
        {
            var handler = new Mock<INotificationHandler<NotificationMock>>();
            handler.Setup(h => h.Handle(It.IsAny<NotificationMock>()));
            return handler;
        });

        var context = fixture.World.CreateActorContext(ctx => ctx.CreateNotificationHandler<NotificationMock>(builder =>
        {
            foreach (var handler in handlers)
            {
                builder.Add(handler.Object);
            }
        }));

        var notification = new NotificationMock(25);

        // act

        context
            .Invoking(ctx => ctx.Publish(notification))
            .Should()
            .NotThrow();

        // assert

        foreach (var handler in handlers)
        {
            handler.Verify(h => h.Handle(notification), Times.Once);
        }

        fixture.World.RemoveActorContext(context);
    }

    [Fact]
    public void NotBeHandled()
    {
        // arrange

        var handler1 = new Mock<INotificationHandler<NotificationMock>>();
        handler1.Setup(h => h.Handle(It.IsAny<NotificationMock>()));

        var context = fixture.World.CreateActorContext(ctx => ctx.AddNotificationHandler(handler1.Object));
        var notification = new NotificationMockNotRegistered(25);

        // act

        context
            .Invoking(ctx => ctx.Publish(notification))
            .Should()
            .Throw<Exception>();

        // assert

        handler1.Verify(h => h.Handle(It.IsAny<NotificationMock>()), Times.Never);

        fixture.World.RemoveActorContext(context);
    }
}