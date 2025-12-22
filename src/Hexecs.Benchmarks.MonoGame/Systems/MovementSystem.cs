using Hexecs.Actors;
using Hexecs.Actors.Systems;
using Hexecs.Benchmarks.MonoGame.Components;
using Hexecs.Threading;
using Hexecs.Worlds;
using Microsoft.Xna.Framework;

namespace Hexecs.Benchmarks.MonoGame.Systems;

public sealed class MovementSystem(
    ActorContext context,
    IParallelWorker worker,
    int width,
    int height
) : UpdateSystem<Position, Velocity>(context, parallelWorker: worker)
{
    private readonly Vector2 _bounds = new(width, height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void Update(
        in ActorRef<Position, Velocity> actor,
        in WorldTime time)
    {
        ref var pos = ref actor.Component1;
        ref var vel = ref actor.Component2;

        pos.Value += vel.Value * time.DeltaTime;

        // Отскоки
        if (pos.Value.X <= 0 || pos.Value.X >= _bounds.X)
        {
            vel.Value.X *= -1;
        }

        if (pos.Value.Y <= 0 || pos.Value.Y >= _bounds.Y)
        {
            vel.Value.Y *= -1;
        }
    }
}