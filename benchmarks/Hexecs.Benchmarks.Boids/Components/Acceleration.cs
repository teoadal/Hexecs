using Hexecs.Actors;

namespace Hexecs.Benchmarks.Boids.Components;

public struct Acceleration : IActorComponent
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Acceleration Create(int x, int y)
    {
        return new Acceleration(new Vector2(x, y));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Acceleration Create(in Point point)
    {
        return new Acceleration(new Vector2(point.X, point.Y));
    }

    public Vector2 Value;

    private Acceleration(Vector2 value)
    {
        Value = value;
    }

    public static implicit operator Acceleration(Vector2 value)
    {
        return new Acceleration(value);
    }
}
