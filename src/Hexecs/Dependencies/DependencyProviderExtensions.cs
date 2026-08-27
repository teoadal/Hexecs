using System.Reflection;

namespace Hexecs.Dependencies;

public static class DependencyProviderExtensions
{
    public static object Activate(
        this IDependencyProvider provider,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        Type implementation)
    {
        ConstructorInfo constructor = DependencyUtils.GetInjectableConstructor(implementation);
        ParameterInfo[] parameters = constructor.GetParameters();
        int parametersLength = parameters.Length;

        if (parametersLength == 0)
        {
            return constructor.Invoke([]);
        }

        var dependencies = new object?[parametersLength];

        for (int i = parametersLength - 1; i >= 0; i--)
        {
            ParameterInfo parameter = parameters[i];
            Type parameterType = parameter.ParameterType;

            object? dependency = provider.GetService(parameterType);

            if (dependency != null)
            {
                dependencies[i] = dependency;
            }
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

        if (service == null)
        {
            DependencyError.ServiceNotRegistered(typeof(TService));
        }

        return service;
    }
}
