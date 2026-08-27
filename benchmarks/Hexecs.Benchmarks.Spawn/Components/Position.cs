using Hexecs.Actors;

namespace Hexecs.Benchmarks.Spawn.Components;

public struct Position : IActorComponent
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Create(int x, int y)
    {
        return new Position(new Vector2(x, y));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Create(in Point point)
    {
        return new Position(new Vector2(point.X, point.Y));
    }

    public Vector2 Value;

    private Position(Vector2 value)
    {
        Value = value;
    }

    public static implicit operator Position(Vector2 value)
    {
        return new Position(value);
    }
}
