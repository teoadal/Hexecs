using System.Diagnostics.Contracts;

namespace Hexecs.Utils;

/// <summary>
/// Обобщенный интерфейс для клонирования структурных типов.
/// Позволяет создавать копии структур, реализующих данный интерфейс.
/// </summary>
/// <typeparam name="T">Тип структуры, которая может быть клонирована. 
/// Должен реализовывать <see cref="ICloneable{T}"/> и быть структурой.</typeparam>
public interface ICloneable<out T>
    where T : struct, ICloneable<T>
{
    /// <summary>
    /// Создает и возвращает копию текущего экземпляра структуры.
    /// </summary>
    /// <returns>Копия текущего экземпляра структуры типа T.</returns>
    [Pure]
    T Clone();
}