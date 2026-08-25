using Hexecs.Pipelines;
using Hexecs.Tests.Mocks;

namespace Hexecs.Tests.Pipelines;

public sealed class QueryShould(PipelineTestFixture fixture) : IClassFixture<PipelineTestFixture>
{
    [Fact]
    public void BeHandled()
    {
        // arrange

        var result = new PipelineResult(111);
        var handler = fixture.CreateQueryHandler<QueryMock, PipelineResult>(cmd => result);
        
        var context = fixture.World.CreateActorContext(ctx => ctx.AddQueryHandler(handler));
        var query = new QueryMock(25);

        // act

        var actualResult = context
            .Invoking(ctx => ctx.Ask<QueryMock, PipelineResult>(query))
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

        var handler = new Mock<IQueryHandler<QueryMock, PipelineResult>>();
        handler.Setup(h => h.Handle(It.IsAny<QueryMock>()));
        
        var context = fixture.World.CreateActorContext(ctx => ctx.AddQueryHandler(handler.Object));
        var query = new QueryMockNotRegistered(25);

        // act

        context.Invoking(ctx => ctx.Ask<QueryMockNotRegistered, PipelineResult>(query))
            .Should()
            .Throw<Exception>();
        
        // assert
        
        handler.Verify(h => h.Handle(It.IsAny<QueryMock>()), Times.Never);

        fixture.World.RemoveActorContext(context);
    }
}