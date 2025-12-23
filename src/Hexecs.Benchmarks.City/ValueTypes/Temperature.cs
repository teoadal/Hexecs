using System.Runtime.CompilerServices;

namespace Hexecs.Benchmarks.Map.ValueTypes;

public readonly struct Temperature
{
    public static Temperature Default => FromCelsius(20);
    
    public static Temperature FromCelsius(byte celsius)
    {
        var raw = (byte)Math.Clamp(celsius + Offset, 0, 255);
        return new Temperature(raw);
    }

    private const byte Offset = 100;
    private readonly byte _raw;

    private Temperature(byte raw) => _raw = raw;

    public float Celsius
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _raw - Offset;
    }

    public bool IsFreezing
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Celsius <= 0;
    }

    public bool IsBoiling
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Celsius >= 100;
    }

    public override string ToString() => $"{Celsius}°C";
}