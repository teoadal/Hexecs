using Hexecsm;

namespace Hexecs.Benchmarks.Noise.Components;

public struct Velocity(Vector2 value) : IComponent
{
    public static Velocity Create(float x, float y)
    {
        return new Velocity(new Vector2(x, y));
    }

    public Vector2 Value = value;

    public static implicit operator Velocity(Vector2 value)
    {
        return new Velocity(value);
    }
}
