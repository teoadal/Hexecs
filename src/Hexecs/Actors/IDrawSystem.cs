using Hexecs.Worlds;

namespace Hexecs.Actors;

/// <summary>
/// Интерфейс системы рисования актёров.
/// </summary>
/// <remarks>
/// Системы рисования позволяют рисовать актёров (<see cref="Actor"/>) и их компоненты в каждом игровом цикле.
/// </remarks>
public interface IDrawSystem
{
    /// <summary>
    /// Контекст актёров (<see cref="Actor"/>), к которому прикреплена система.
    /// </summary>
    ActorContext Context { get; }

    /// <summary>
    /// Получает или устанавливает флаг активности системы.
    /// Если значение равно false, система не будет рисовать актёров (<see cref="Actor"/>).
    /// </summary>
    bool Enabled { get; set; }

    /// <summary>
    /// Метод рисования, вызываемый на каждом игровом цикле.
    /// </summary>
    /// <param name="time">Информация о времени мира, предоставляющая временные метрики текущего кадра.</param>
    void Draw(in WorldTime time);
}