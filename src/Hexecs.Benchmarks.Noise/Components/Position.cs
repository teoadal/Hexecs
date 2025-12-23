using Hexecs.Actors;
using Microsoft.Xna.Framework;

namespace Hexecs.Benchmarks.Noise.Components;

public struct Position(Vector2 value) : IActorComponent
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Create(int x, int y) => new(new Vector2(x, y));
    
    public Vector2 Value = value;

    public static implicit operator Position(Vector2 value) => new(value);
}