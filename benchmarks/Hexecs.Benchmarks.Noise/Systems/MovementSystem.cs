using Hexecs.Benchmarks.Noise.Components;

using Hexecsm;
using Hexecsm.Systems;
using Hexecsm.Worlds;

namespace Hexecs.Benchmarks.Noise.Systems;

public sealed class MovementSystem(
    World context,
    int width,
    int height) : ParallelUpdateSystem<Position, Velocity>(context)
{
    private readonly Vector2 _bounds = new Vector2(width, height);

    protected override void Update(
        in ActorRef<Position, Velocity> actor,
        in WorldTime time)
    {
        ref Vector2 pos = ref actor.Component1.Value;
        ref Vector2 vel = ref actor.Component2.Value;

        pos += vel * time.DeltaTime;

        // Отскоки
        if (pos.X <= 0 || pos.X >= _bounds.X)
        {
            vel.X *= -1;
        }

        if (pos.Y <= 0 || pos.Y >= _bounds.Y)
        {
            vel.Y *= -1;
        }
    }
}
