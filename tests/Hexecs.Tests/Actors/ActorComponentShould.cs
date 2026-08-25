using Hexecs.Tests.Mocks.ActorComponents;

namespace Hexecs.Tests.Actors;

public sealed class ActorComponentShould(ActorTestFixture fixture) : IClassFixture<ActorTestFixture>
{
    [Fact(DisplayName = "Должен быть вызван Dispose")]
    public void BeDisposedAfterDestroyComponent()
    {
        var monitor = new Mock<IDisposable>();
        var actor = fixture.CreateActor<DisposableComponent>(component1: new DisposableComponent(monitor.Object));

        actor.Destroy();

        monitor.Verify(m => m.Dispose(), Times.Once());
    }
}