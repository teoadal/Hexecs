using System.Reflection;

namespace Hexecs.Dependencies;

internal static class DependencyUtils
{
    public static ConstructorInfo GetInjectableConstructor(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        Type type)
    {
        var constructors = type.GetConstructors();
        foreach (var constructor in constructors)
        {
            if (constructor.IsPrivate || constructor.IsStatic) continue;
            return constructor;
        }

        DependencyError.InjectableConstructorNotFound(type);
        return null;
    }
}