using Hexecs.Actors;
using Hexecs.Actors.Systems;
using Hexecs.Benchmarks.Spawn.Components;
using Hexecs.Threading;
using Hexecs.Worlds;

namespace Hexecs.Benchmarks.Spawn.Systems;

public sealed class MovementSystem : UpdateSystem<Position, Velocity>
{
    private const float Gravity = 350f;

    // Коэффициент упругости: 0.75f означает, что частица теряет 25% энергии при каждом ударе
    private const float BounceRestitution = 0.75f;

    private readonly float _width;
    private readonly float _height;

    // Передаем полные размеры экрана из настроек SpawnGame (1280x720)
    public MovementSystem(ActorContext context, IParallelWorker parallelWorker, int width, int height)
        : base(context, parallelWorker: parallelWorker)
    {
        _width = width;
        _height = height;
    }

    [SkipLocalsInit]
    protected override void Update(ActorFilter<Position, Velocity>.SkipTakeEnumerator batch, in WorldTime time)
    {
        float dt = time.DeltaTime;

        foreach (ActorRef<Position, Velocity> actor in batch)
        {
            ref Vector2 position = ref actor.Component1.Value;
            ref Vector2 velocity = ref actor.Component2.Value;

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
