namespace Hexecs.Dependencies;

public interface IDependencyCollection
{
    IDependencyCollection Add(DependencyLifetime lifetime, Type contract, Func<IDependencyProvider, object> resolver);

    IDependencyCollection AddRegistrar(IDependencyRegistrar registrar);

    IDependencyCollection Singleton(Type contract, Func<IDependencyProvider, object> resolver);

    IDependencyCollection Singleton<T>(Func<IDependencyProvider, T> resolver) where T : class;

    IDependencyCollection Scoped(Type contract, Func<IDependencyProvider, object> resolver);

    IDependencyCollection Scoped<T>(Func<IDependencyProvider, T> resolver) where T : class;

    IDependencyCollection Transient(Type contract, Func<IDependencyProvider, object> resolver);

    IDependencyCollection Transient<T>(Func<IDependencyProvider, T> resolver) where T : class;
}