using System.Runtime.CompilerServices;

namespace Hexecs.Benchmarks.Map.Terrains.ValueTypes;

public readonly struct Elevation
{
    public static Elevation Default => FromValue(10);
    
    public static Elevation FromValue(int value)
    {
        var raw = (byte)Math.Clamp(value + Offset, 0, 255);
        return new Elevation(raw);
    }

    public static Elevation SeaLevel
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(Offset);
    }

    private const byte Offset = 100; // sea level
    private readonly byte _raw;

    private Elevation(byte raw) => _raw = raw;

    public int Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _raw - Offset;
    }

    /// <summary>
    /// Находится ли уровень ниже базового (уровня моря).
    /// </summary>
    public bool IsBelowSeaLevel
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _raw < 100;
    }

    /// <summary>
    /// Является ли это возвышенностью (холмом).
    /// </summary>
    public bool IsHighland
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _raw > 150;
    }

    public bool IsSeaLevel
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _raw == Offset;
    }

    public override string ToString() => $"{Value}m";
}