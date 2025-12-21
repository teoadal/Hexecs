namespace Hexecs.Worlds;

/// <summary>
/// Структура, представляющая время в игровом мире.
/// Оптимизирована для AOT и минимального footprint в памяти.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly struct WorldTime(int cycle, long elapsedTicks, long totalTicks)
{
    public static WorldTime Zero
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => default;
    }
    
    private const float TicksToSeconds = 1f / TimeSpan.TicksPerSecond;

    /// <summary>
    /// Общее количество тактов с момента создания мира.
    /// </summary>
    public readonly long TotalTicks = totalTicks;

    /// <summary>
    /// Количество тактов, прошедших с момента последнего обновления.
    /// </summary>
    public readonly long ElapsedTicks = elapsedTicks;

    /// <summary>
    /// Время кадра в секундах. Кэшировано для производительности.
    /// </summary>
    public readonly float DeltaTime = elapsedTicks * TicksToSeconds;

    /// <summary>
    /// Текущий цикл обновления мира.
    /// </summary>
    public readonly int Cycle = cycle;

    public TimeSpan Elapsed
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => TimeSpan.FromTicks(ElapsedTicks);
    }

    public TimeSpan Total
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => TimeSpan.FromTicks(TotalTicks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public WorldTime(int cycle, TimeSpan elapsed, TimeSpan total)
        : this(cycle, elapsed.Ticks, total.Ticks)
    {
    }
}