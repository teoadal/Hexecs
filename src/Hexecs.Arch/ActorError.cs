namespace Hexecs.Arch;

/// <summary>
/// Статический класс, содержащий методы для обработки ошибок, связанных с актёрами.
/// Предоставляет удобные методы для генерации типовых исключений при работе с актёрами.
/// </summary>
internal static class ActorError
{
    /// <summary>
    /// Генерирует исключение, когда тип компонента актёра с указанным идентификатором не найден.
    /// </summary>
    /// <param name="id">Идентификатор типа компонента</param>
    [DoesNotReturn]
    public static void ComponentTypeNotFound(uint id)
    {
        throw new Exception($"Actor component type with id '{id}' isn't found");
    }
}