namespace Hexecs.Dependencies;

public static class DependencyProviderExtensions
{
    public static object Activate(
        this IDependencyProvider provider,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        Type implementation)
    {
        var constructor = DependencyUtils.GetInjectableConstructor(implementation);
        var parameters = constructor.GetParameters();
        var parametersLength = parameters.Length;

        if (parametersLength == 0)
        {
            return constructor.Invoke([]);
        }

        var dependencies = new object?[parametersLength];
        for (var i = parametersLength - 1; i >= 0; i--)
        {
            var parameter = parameters[i];
            var parameterType = parameter.ParameterType;

            var dependency = provider.GetService(parameterType);
            if (dependency != null) dependencies[i] = dependency;
            else
            {
                dependencies[i] = parameter.HasDefaultValue
                    ? parameter.DefaultValue
                    : DependencyError.ServiceNotRegistered(parameterType, implementation);
            }
        }

        return constructor.Invoke(dependencies);
    }

    public static TService GetRequiredService<TService>(this IDependencyProvider provider)
        where TService : class
    {
        var service = provider.GetService<TService>();
        if (service == null) DependencyError.ServiceNotRegistered(typeof(TService));

        return service;
    }
}