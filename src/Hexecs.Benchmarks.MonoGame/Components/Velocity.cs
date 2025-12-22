using Hexecs.Actors;
using Microsoft.Xna.Framework;

namespace Hexecs.Benchmarks.MonoGame.Components;

public struct Velocity(Vector2 value) : IActorComponent
{
    public static Velocity Create(float x, float y) => new(new Vector2(x, y));
    
    public Vector2 Value = value;

    public static implicit operator Velocity(Vector2 value) => new(value);
}