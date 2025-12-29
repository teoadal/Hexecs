namespace Hexecs.Worlds;

/// <summary>
/// Класс, реализующий генерацию случайных чисел для имитации броска костей.
/// Использует линейный конгруэнтный метод для генерации псевдослучайных чисел.
/// </summary>
public sealed class Dice(int? seed = null)
{
    /// <summary>
    /// Текущее значение семени для генерации случайных чисел.
    /// Инициализируется случайным значением, если не указано явно.
    /// </summary>
    private int _seed = seed ?? Random.Shared.Next();

    /// <summary>
    /// Генерирует следующее псевдослучайное число.
    /// Использует алгоритм: seed = (214013 * seed + 2531011)
    /// Возвращает младшие 15 бит результата.
    /// </summary>
    /// <returns>Псевдослучайное число в диапазоне [0, 32767]</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetNext()
    {
        var seed = Interlocked.Exchange(ref _seed, 214013 * _seed + 2531011);
        return (seed >> 16) & 0x7FFF;
    }

    /// <summary>
    /// Генерирует псевдослучайное число в указанном диапазоне [start, end].
    /// </summary>
    /// <param name="start">Нижняя граница диапазона</param>
    /// <param name="end">Верхняя граница диапазона</param>
    /// <returns>Целое число в указанном диапазоне</returns>
    public int GetNext(int start, int end)
    {
        if (start == end) return start;
        if (end < start) (end, start) = (start, end);

        var maxValue = (uint)(end - start);
        var value = (uint)GetNext() % maxValue;
        return (int)value + start;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double GetNextDouble() => GetNext() / 32768.0;

    #region Roll

    /// <summary>
    /// Имитирует бросок кости с 50% вероятностью успеха.
    /// </summary>
    /// <returns>true - успех, false - провал</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Roll() => GetNext(0, 100) > 50;

    /// <summary>
    /// Имитирует бросок кости с указанной вероятностью успеха.
    /// </summary>
    /// <param name="success">Вероятность успеха в процентах (0-100)</param>
    /// <returns>true - успех, false - провал</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Roll(int success)
    {
        if (success == 100) return true;
        return success > 0 && GetNext(0, 100) <= success;
    }

    #endregion
}