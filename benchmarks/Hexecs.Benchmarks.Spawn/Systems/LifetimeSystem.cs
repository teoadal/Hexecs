using Hexecs.Actors;
using Hexecs.Actors.Systems;
using Hexecs.Benchmarks.Spawn.Components;
using Hexecs.Threading;
using Hexecs.Worlds;
using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;

namespace Hexecs.Benchmarks.Spawn.Systems;

internal sealed class LifetimeSystem(
    ActorContext context,
    IParallelWorker parallelWorker) : UpdateSystem<Lifetime, CircleColor>(context, parallelWorker: parallelWorker)
{
    [SkipLocalsInit]
    protected override void Update(ActorFilter<Lifetime, CircleColor>.SkipTakeEnumerator batch, in WorldTime time)
    {
        float dt = time.DeltaTime;

        foreach (ActorRef<Lifetime, CircleColor> actor in batch)
        {
            ref Lifetime lifetime = ref actor.Component1;
            ref CircleColor circleColor = ref actor.Component2;

            lifetime.Value -= dt;
            if (lifetime.Value <= 0f)
            {
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
