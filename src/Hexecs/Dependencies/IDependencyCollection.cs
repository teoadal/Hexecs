namespace Hexecs.Dependencies;

public interface IDependencyCollection
{
    IDependencyCollection Add(DependencyLifetime lifetime, Type contract, Func<IDependencyProvider, object> resolver);

    IDependencyCollection AddRegistrar(IDependencyRegistrar registrar);

    IDependencyCollection UseSingleton(Type contract, Func<IDependencyProvider, object> resolver);

    IDependencyCollection UseSingleton<T>(Func<IDependencyProvider, T> resolver) where T : class;

    IDependencyCollection UseScoped(Type contract, Func<IDependencyProvider, object> resolver);

    IDependencyCollection UseScoped<T>(Func<IDependencyProvider, T> resolver) where T : class;

    IDependencyCollection UseTransient(Type contract, Func<IDependencyProvider, object> resolver);

    IDependencyCollection UseTransient<T>(Func<IDependencyProvider, T> resolver) where T : class;
}