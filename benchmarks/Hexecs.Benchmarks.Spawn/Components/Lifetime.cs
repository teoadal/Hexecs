using Hexecsm;

namespace Hexecs.Benchmarks.Spawn.Components;

public struct Lifetime : IComponent
{
    public static Lifetime Create(float seconds)
    {
        return new Lifetime(seconds);
    }

    public float Value;

    private Lifetime(float value)
    {
        Value = value;
    }
}
