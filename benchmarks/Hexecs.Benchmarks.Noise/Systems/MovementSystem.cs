using Hexecs.Actors;
using Hexecs.Actors.Systems;
using Hexecs.Benchmarks.Noise.Components;
using Hexecs.Threading;
using Hexecs.Worlds;

namespace Hexecs.Benchmarks.Noise.Systems;

public sealed class MovementSystem(
    ActorContext context,
    IParallelWorker worker,
    int width,
    int height) : UpdateSystem<Position, Velocity>(context, parallelWorker: worker)
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
