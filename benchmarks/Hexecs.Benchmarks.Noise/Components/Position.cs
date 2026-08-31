using Hexecsm;

namespace Hexecs.Benchmarks.Noise.Components;

public struct Position(Vector2 value) : IComponent
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Create(int x, int y)
    {
        return new Position(new Vector2(x, y));
    }

    public Vector2 Value = value;

    public static implicit operator Position(Vector2 value)
    {
        return new Position(value);
    }
}
