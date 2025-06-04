namespace Hexecs.Worlds;

internal static class WorldError
{
    /// <summary>
    /// Выбрасывает ошибку, если контекст акёров не найден по указанному <paramref name="id"/>.
    /// </summary>
    /// <param name="id">Идентификатор контекста</param>
    [DoesNotReturn]
    public static void ActorContextNotFound(int id)
    {
        throw new Exception($"Actor context with id {id} isn't found");
    }
    
    [DoesNotReturn]
    public static void InvalidState(WorldState currentState)
    {
        throw new Exception($"World has invalid state '{currentState}' for execute it");
    }
}