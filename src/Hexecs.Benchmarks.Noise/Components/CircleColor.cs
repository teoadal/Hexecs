using Hexecs.Actors;
using Microsoft.Xna.Framework;

namespace Hexecs.Benchmarks.Noise.Components;

public readonly struct CircleColor(Color value) : IActorComponent
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CircleColor CreateRgba(Random random, byte? alpha = null)
    {
        var r = (byte)random.Next(256);
        var g = (byte)random.Next(256);
        var b = (byte)random.Next(256);
        
        return new CircleColor(new Color(r, g, b, alpha ?? 255));
    }
    
    public readonly Color Value = value;

    public static implicit operator CircleColor(Color value) => new(value);
}