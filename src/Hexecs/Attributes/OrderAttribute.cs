using System.Reflection;

namespace Hexecs.Attributes;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class OrderAttribute(int value) : Attribute
{
    public static int TryGetValue(Type type, int defaultValue = int.MaxValue)
    {
        var attribute = type.GetCustomAttribute<OrderAttribute>();
        return attribute?.Value ?? defaultValue;
    }

    public readonly int Value = value;
}