namespace Hexecs.Dependencies;

[DebuggerDisplay("{ServiceType.Name} ({Index})")]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
internal readonly struct DependencyKey(Type serviceType, int index) : IEquatable<DependencyKey>
{
    public const int DefaultSlot = 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DependencyKey First(Type serviceType)
    {
        return new DependencyKey(serviceType, DefaultSlot);
    }

    public readonly int Index = index;
    public readonly Type ServiceType = serviceType;

    public bool Equals(DependencyKey other)
    {
        return Index == other.Index && ServiceType == other.ServiceType;
    }

    public override bool Equals(object? obj)
    {
        return obj is DependencyKey key && Equals(key);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Index, ServiceType);
    }
}
