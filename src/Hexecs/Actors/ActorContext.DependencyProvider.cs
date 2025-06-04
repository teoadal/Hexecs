using Hexecs.Dependencies;

namespace Hexecs.Actors;

public sealed partial class ActorContext : IDependencyProvider
{
    private readonly DependencyProvider _dependencyProvider;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object? GetService(Type contract) => _dependencyProvider.GetService(contract);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TService? GetService<TService>() where TService : class => _dependencyProvider.GetService<TService>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TService[] GetServices<TService>() where TService : class => _dependencyProvider.GetServices<TService>();
}