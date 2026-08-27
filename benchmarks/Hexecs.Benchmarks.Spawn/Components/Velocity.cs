using Hexecs.Actors;

namespace Hexecs.Benchmarks.Spawn.Components;

public struct Velocity : IActorComponent
{
    public static Velocity Create(float x, float y)
    {
        return new Velocity(new Vector2(x, y));
    }

    public Vector2 Value;

    private Velocity(Vector2 value)
    {
        Value = value;
    }

    public static implicit operator Velocity(Vector2 value)
    {
        return new Velocity(value);
    }
}
