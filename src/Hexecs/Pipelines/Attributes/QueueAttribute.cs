using System.Reflection;

namespace Hexecs.Pipelines.Attributes;

[AttributeUsage(AttributeTargets.Struct)]
public sealed class QueueAttribute(string name) : Attribute
{
    public readonly string Name = name;

    public static bool TryGetName(Type instanceType, out string name)
    {
        var attribute = instanceType.GetCustomAttribute<QueueAttribute>();
        if (attribute == null)
        {
            name = string.Empty;
            return false;
        }

        name = attribute.Name;
        return true;
    }
}