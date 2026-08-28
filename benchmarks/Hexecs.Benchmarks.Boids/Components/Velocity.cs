using Hexecs.Actors;

namespace Hexecs.Benchmarks.Boids.Components;

public struct Velocity : IActorComponent
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Velocity Create(int x, int y)
    {
        return new Velocity(new Vector2(x, y));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Velocity Create(in Point point)
    {
        return new Velocity(new Vector2(point.X, point.Y));
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
