using Hexecs.Pipelines;
using Hexecs.Tests.Mocks;

namespace Hexecs.Tests.Pipelines;

public sealed class MessageShould(PipelineTestFixture fixture) : IClassFixture<PipelineTestFixture>
{
    [Fact]
    public void BeHandled()
    {
        // arrange

        var handler = new Mock<IMessageHandler<MessageMock>>();
        handler.Setup(h => h.Handle(It.IsAny<MessageMock>()));

        var context = fixture.World.CreateActorContext(ctx => ctx.AddMessageHandler(handler.Object));
        var message = new MessageMock(25);

        context.Invoking(ctx => ctx.Send(message))
            .Should()
            .NotThrow();

        var queue = context.GetMessageQueue<MessageMock>();

        // act

        queue.Execute();

        // assert

        handler.Verify(h => h.Handle(message), Times.Once);
    }

    [Fact]
    public void NotBeHandled()
    {
        // arrange

        var handler = new Mock<IMessageHandler<MessageMock>>();
        handler.Setup(h => h.Handle(It.IsAny<MessageMock>()));

        var context = fixture.World.CreateActorContext(ctx => ctx.AddMessageHandler(handler.Object));
        var message = new MessageMockNotRegistered(25);

        // act

        context.Invoking(ctx => ctx.Send(message))
            .Should()
            .Throw<Exception>();

        context.Invoking(ctx => ctx.GetMessageQueue<MessageMockNotRegistered>())
            .Should()
            .Throw<Exception>();

        // assert

        handler.Verify(h => h.Handle(It.IsAny<MessageMock>()), Times.Never);
    }
}