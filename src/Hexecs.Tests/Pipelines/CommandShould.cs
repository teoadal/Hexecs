using Hexecs.Pipelines;
using Hexecs.Tests.Mocks;

namespace Hexecs.Tests.Pipelines;

public sealed class CommandShould(PipelineTestFixture fixture) : IClassFixture<PipelineTestFixture>
{
    [Fact]
    public void BeHandled()
    {
        // arrange

        var handler = new Mock<ICommandHandler<CommandMock>>();
        handler.Setup(h => h.Handle(It.IsAny<CommandMock>()));

        var context = fixture.World.CreateActorContext(ctx => ctx.AddCommandHandler(handler.Object));
        var command = new CommandMock(25);

        // act

        context.Invoking(ctx => ctx.Execute(command))
            .Should()
            .NotThrow();

        // assert

        handler.Verify(h => h.Handle(command), Times.Once);

        fixture.World.RemoveActorContext(context);
    }

    [Fact]
    public void BeWithResultHandled()
    {
        // arrange

        var result = new PipelineResult(111);
        var handler = fixture.CreateCommandHandler<CommandMockWithResult, PipelineResult>(cmd => result);
        
        var context = fixture.World.CreateActorContext(ctx => ctx.AddCommandHandler(handler));
        var command = new CommandMockWithResult(25);

        // act

        var actualResult = context
            .Invoking(ctx => ctx.Execute<CommandMockWithResult, PipelineResult>(command))
            .Should()
            .NotThrow()
            .Which;

        // assert

        actualResult
            .Should()
            .Be(result);

        fixture.World.RemoveActorContext(context);
    }
    
    [Fact]
    public void NotBeHandled()
    {
        // arrange

        var handler = new Mock<ICommandHandler<CommandMockWithResult, PipelineResult>>();
        handler.Setup(h => h.Handle(It.IsAny<CommandMockWithResult>()));

        var context = fixture.World.CreateActorContext(ctx => ctx.AddCommandHandler(handler.Object));
        var command = new CommandMock(25);

        // act

        context.Invoking(ctx => ctx.Execute(command))
            .Should()
            .Throw<Exception>();

        // assert

        handler.Verify(h => h.Handle(It.IsAny<CommandMockWithResult>()), Times.Never);

        fixture.World.RemoveActorContext(context);
    }
}