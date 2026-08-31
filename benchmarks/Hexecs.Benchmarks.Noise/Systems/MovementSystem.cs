using Hexecs.Benchmarks.Noise.Components;

using Hexecsm;
using Hexecsm.Accessors;
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
        KeyAccessor batchKeys,
        in ValueAccessor<Position> components1,
        in ValueAccessor<Velocity> components2,
        in WorldTime worldTime)
    {
        foreach (ActorId actorId in batchKeys.AsReadOnlySpan())
        {
            ref Vector2 pos = ref components1.GetValue(actorId).Value;
            ref Vector2 vel = ref components2.GetValue(actorId).Value;

            pos += vel * worldTime.DeltaTime;

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
}
