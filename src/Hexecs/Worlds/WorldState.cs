namespace Hexecs.Worlds;

/// <summary>
/// Состояние мира
/// </summary>
public enum WorldState
{
    /// <summary>
    /// Состояние не определено
    /// </summary>
    None,
    /// <summary>
    /// Отрисовка актёров
    /// </summary>
    Draw,
    /// <summary>
    /// Обновление актёров
    /// </summary>
    Update
}