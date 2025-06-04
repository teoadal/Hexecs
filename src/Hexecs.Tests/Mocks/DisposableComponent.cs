namespace Hexecs.Tests.Mocks;

public readonly struct DisposableComponent(IDisposable monitor) : IActorComponent, IDisposable
{
    public void Dispose() => monitor.Dispose();
}