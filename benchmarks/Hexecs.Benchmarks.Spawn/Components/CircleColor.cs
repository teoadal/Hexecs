using Hexecs.Actors;
using Hexecs.Worlds;

namespace Hexecs.Benchmarks.Spawn.Components;

public struct CircleColor : IActorComponent
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CircleColor CreateRgba(Dice random, byte? alpha = null)
    {
        var r = (byte)random.GetNext(0, 256);
        var g = (byte)random.GetNext(0, 256);
        var b = (byte)random.GetNext(0, 256);

        return new CircleColor(new Color(r, g, b, alpha ?? 255));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CircleColor CreateFromVelocity(Vector2 velocity)
    {
        // Вычисляем общую скорость частицы (длину вектора)
        float speed = velocity.Length();

        // Нормализуем скорость в диапазон от 0.0 до 1.0
        // Предполагаем, что максимальная стартовая скорость в фонтане около 500-600
        float factor = MathHelper.Clamp(speed / 550f, 0f, 1f);

        // Интерполяция между холодным синим (медленная скорость) и горячим оранжевым (высокая скорость)
        var coldColor = new Color(0, 128, 255);   // Кислотно-синий
        var hotColor = new Color(255, 110, 0);   // Огненно-рыжий

        Color finalColor = Color.Lerp(coldColor, hotColor, factor);

        return new CircleColor(finalColor);
    }

    public Color Value;

    private CircleColor(Color value)
    {
        Value = value;
    }

    public static implicit operator CircleColor(Color value)
    {
        return new CircleColor(value);
    }
}
