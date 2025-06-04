namespace Hexecs.Worlds;

/// <summary>
/// Структура, представляющая время в игровом мире.
/// </summary>
/// <remarks>
/// Содержит информацию о текущем цикле, прошедшем и общем времени.
/// </remarks>
public readonly struct WorldTime
{
    /// <summary>
    /// Возвращает нулевое (начальное) значение времени мира.
    /// </summary>
    public static WorldTime Zero
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(0, TimeSpan.Zero, TimeSpan.Zero);
    }

    public float DeltaTime
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (float)Elapsed.TotalSeconds;
    }

    /// <summary>
    /// Текущий цикл обновления мира.
    /// </summary>
    public readonly int Cycle;

    /// <summary>
    /// Время, прошедшее с момента последнего обновления.
    /// </summary>
    public readonly TimeSpan Elapsed;

    /// <summary>
    /// Общее время с момента создания мира.
    /// </summary>
    public readonly TimeSpan Total;

    /// <summary>
    /// Инициализирует новый экземпляр структуры WorldTime с указанными значениями.
    /// </summary>
    /// <param name="cycle">Номер текущего цикла обновления.</param>
    /// <param name="elapsed">Время, прошедшее с момента последнего обновления.</param>
    /// <param name="total">Общее время с момента создания мира.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public WorldTime(int cycle, TimeSpan elapsed, TimeSpan total)
    {
        Cycle = cycle;
        Elapsed = elapsed;
        Total = total;
    }

    /// <summary>
    /// Инициализирует новый экземпляр структуры WorldTime с указанными значениями.
    /// </summary>
    /// <param name="cycle">Номер текущего цикла обновления.</param>
    /// <param name="elapsedTicks">Количество тактов, прошедших с момента последнего обновления.</param>
    /// <param name="totalTicks">Общее количество тактов с момента создания мира.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public WorldTime(int cycle, long elapsedTicks, long totalTicks)
    {
        Cycle = cycle;
        Elapsed = TimeSpan.FromTicks(elapsedTicks);
        Total = TimeSpan.FromTicks(totalTicks);
    }
}