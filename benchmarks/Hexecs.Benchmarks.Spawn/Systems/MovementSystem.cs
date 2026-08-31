using Hexecs.Benchmarks.Spawn.Components;

using Hexecsm;
using Hexecsm.Accessors;
using Hexecsm.Systems;
using Hexecsm.Worlds;

namespace Hexecs.Benchmarks.Spawn.Systems;

public sealed class MovementSystem(World context, int width, int height) : ParallelUpdateSystem<Position, Velocity>(context)
{
    private const float Gravity = 350f;

    // Коэффициент упругости: 0.75f означает, что частица теряет 25% энергии при каждом ударе
    private const float BounceRestitution = 0.75f;

    private readonly float _width = width;
    private readonly float _height = height;

    [SkipLocalsInit]
    protected override void Update(
        KeyAccessor batchKeys,
        in ValueAccessor<Position> components1,
        in ValueAccessor<Velocity> components2,
        in WorldTime worldTime)
    {
        float dt = worldTime.DeltaTime;

        foreach (ActorId actorId in batchKeys.AsReadOnlySpan())
        {
            ref Vector2 position = ref components1.GetValue(actorId).Value;
            ref Vector2 velocity = ref components2.GetValue(actorId).Value;

            // 1. Физика гравитации
            velocity.Y += Gravity * dt;

            // 2. Интеграция движения
            position.X += velocity.X * dt;
            position.Y += velocity.Y * dt;

            // 3. Отскок от нижней границы (ПОЛ)
            if (position.Y >= _height)
            {
                position.Y = _height;

                if (velocity.Y > 0f)
                {
                    velocity.Y = -velocity.Y * BounceRestitution;
                }
            }

            // 4. Отскок от левой границы экрана
            if (position.X <= 0f)
            {
                position.X = 0f;

                if (velocity.X < 0f)
                {
                    velocity.X = -velocity.X * BounceRestitution;
                }
            }

            // 5. Отскок от правой границы экрана
            else if (position.X >= _width)
            {
                position.X = _width;

                if (velocity.X > 0f)
                {
                    velocity.X = -velocity.X * BounceRestitution;
                }
            }
        }
    }
}
