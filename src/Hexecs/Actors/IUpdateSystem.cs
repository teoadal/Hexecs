using Hexecs.Worlds;

namespace Hexecs.Actors;

/// <summary>
/// Интерфейс системы обновления для актёров.
/// </summary>
/// <remarks>
/// Системы обновления позволяют обрабатывать актёров (<see cref="Actor"/>) и их компоненты в каждом игровом цикле.
/// </remarks>
public interface IUpdateSystem
{
    /// <summary>
    /// Контекст актёров (<see cref="Actor"/>), к которому прикреплена система.
    /// </summary>
    ActorContext Context { get; }

    /// <summary>
    /// Получает или устанавливает флаг активности системы.
    /// Если значение равно false, система не будет обновлять актёров (<see cref="Actor"/>).
    /// </summary>
    bool Enabled { get; set; }

    /// <summary>
    /// Метод обновления, вызываемый на каждом игровом цикле.
    /// </summary>
    /// <param name="time">Информация о времени мира, предоставляющая временные метрики текущего кадра.</param>
    void Update(in WorldTime time);
}