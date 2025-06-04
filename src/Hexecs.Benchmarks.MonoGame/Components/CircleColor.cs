using Hexecs.Actors;
using Microsoft.Xna.Framework;

namespace Hexecs.Benchmarks.MonoGame.Components;

public readonly struct CircleColor(Color value) : IActorComponent
{
    public readonly Color Value = value;

    public static implicit operator CircleColor(Color value) => new(value);
}