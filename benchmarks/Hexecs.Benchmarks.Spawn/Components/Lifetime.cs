using Hexecs.Actors;

namespace Hexecs.Benchmarks.Spawn.Components;

public struct Lifetime : IActorComponent
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
