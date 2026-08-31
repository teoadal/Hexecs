using Hexecs.Benchmarks.Spawn.Components;

using Hexecsm;
using Hexecsm.Accessors;
using Hexecsm.Systems;
using Hexecsm.Worlds;

namespace Hexecs.Benchmarks.Spawn.Systems;

internal sealed class LifetimeSystem(World world) : ParallelUpdateSystem<Lifetime, CircleColor>(world)
{
    private World _world = world;

    [SkipLocalsInit]
    protected override void Update(
        KeyAccessor batchKeys,
        in ValueAccessor<Lifetime> components1,
        in ValueAccessor<CircleColor> components2,
        in WorldTime worldTime)
    {
        float dt = worldTime.DeltaTime;

        foreach (ActorId actorId in batchKeys.AsReadOnlySpan())
        {
            ref Lifetime lifetime = ref components1.GetValue(actorId);
            ref CircleColor circleColor = ref components2.GetValue(actorId);

            lifetime.Value -= dt;

            if (lifetime.Value <= 0f)
            {
                _world.DestroyActor(actorId);

                continue;
            }

            Color currentColor = circleColor.Value;

            // 1. Плавное затухание альфы на основе остатка жизни (процент от максимума в 4.5 сек)
            float lifePercentage = MathHelper.Clamp(lifetime.Value / 4.5f, 0f, 1f);
            var finalAlpha = (byte)(lifePercentage * 255);

            // 2. Эффект остывания: со временем сдвигаем цвет в сторону фиолетового/синего спектра
            // Уменьшаем красный канал по мере старения частицы
            var r = (byte)MathHelper.Clamp(currentColor.R - (100f * dt), 0f, 255f);

            // Слегка подсвечиваем синий канал для старых частиц
            var b = (byte)MathHelper.Clamp(currentColor.B + (50f * dt), 0f, 255f);

            circleColor = new Color(r, currentColor.G, b, finalAlpha);
        }
    }
}
