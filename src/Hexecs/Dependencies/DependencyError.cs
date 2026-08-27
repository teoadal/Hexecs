namespace Hexecs.Dependencies;

internal static class DependencyError
{
    [DoesNotReturn]
    public static void CircularDependency(Type request, HashSet<Type> progress)
    {
        string path = string.Join(" -> ", progress.Reverse().Select(TypeOf.GetTypeName));

        throw new Exception($"Detected circular dependency of service '{TypeOf.GetTypeName(request)}' ({path})");
    }

    [DoesNotReturn]
    public static void Disposed()
    {
        throw new ObjectDisposedException("Dependency provider is disposed");
    }

    [DoesNotReturn]
    public static void InjectableConstructorNotFound(Type request)
    {
        throw new Exception($"Injectable constructor not found in type '{TypeOf.GetTypeName(request)}'");
    }

    [DoesNotReturn]
    public static T NotSupportedLifetime<T>(DependencyLifetime lifetime)
    {
        throw new Exception($"Lifetime '{lifetime}' isn't supported");
    }

    [DoesNotReturn]
    public static object ServiceNotRegistered(Type required, Type? request = null)
    {
        string service = TypeOf.GetTypeName(required);

        throw request == null
            ? new Exception($"Service '{service}' isn't registered")
            : new Exception($"Service '{service}' isn't registered for resolve {TypeOf.GetTypeName(request)}");
    }

    [DoesNotReturn]
    public static void ResolverNotFound(Type contract, DependencyLifetime lifetime)
    {
        string service = TypeOf.GetTypeName(contract);

        throw new Exception($"Service resolver for '{service}' with lifetime {lifetime} isn't registered");
    }
}
