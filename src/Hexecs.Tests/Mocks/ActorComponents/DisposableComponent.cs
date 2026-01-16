namespace Hexecs.Tests.Mocks.ActorComponents;

public readonly struct DisposableComponent(IDisposable monitor) : IActorComponent, IDisposable
{
    public void Dispose() => monitor.Dispose();
}