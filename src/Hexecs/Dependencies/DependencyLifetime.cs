namespace Hexecs.Dependencies;

public enum DependencyLifetime : byte
{
    Singleton,
    Scoped,
    Transient,
}