using Hexecs.Actors;
using Hexecs.Worlds;

namespace Hexecs.Benchmarks.Boids.Components;

public struct CircleColor : IActorComponent
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CircleColor CreateRgba(Dice random, byte? alpha = null)
    {
        var r = (byte)random.GetNext(0, 256);
        var g = (byte)random.GetNext(0, 256);
        var b = (byte)random.GetNext(0, 256);

        return new CircleColor(new Color(r, g, b, alpha ?? 255));
    }

    public Color Value;

    private CircleColor(Color value)
    {
        Value = value;
    }

    public static implicit operator CircleColor(Color value)
    {
        return new CircleColor(value);
    }
}
