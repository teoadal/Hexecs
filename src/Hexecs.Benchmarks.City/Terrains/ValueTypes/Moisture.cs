using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Hexecs.Benchmarks.Map.Terrains.ValueTypes;

[DebuggerDisplay("{Value:F2}%")]
public readonly struct Moisture
{
    public static Moisture Default
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(Offset);
    }

    public static Moisture FromValue(float value)
    {
        var raw = (byte)Math.Clamp(value + Offset, 0, 255);
        return new Moisture(raw);
    }

    private const byte Offset = 100;
    private readonly byte _raw;

    private Moisture(byte raw) => _raw = raw;

    public float Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _raw - Offset;
    }

    public bool IsDry
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Value < -50;
    }

    public bool IsSoaked
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Value > 50;
    }
}