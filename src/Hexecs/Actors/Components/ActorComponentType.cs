namespace Hexecs.Actors.Components;

internal static class ActorComponentType
{
    private static readonly Dictionary<Type, ushort> ComponentTypes = new(128, ReferenceComparer<Type>.Instance);
    private static readonly Lock LockObj = new();

    private static ushort _nextId;

    /// <summary>
    /// Получает уникальный идентификатор для указанного типа компонента.
    /// </summary>
    /// <param name="type">Тип компонента</param>
    /// <returns>Уникальный идентификатор типа компонента</returns>
    public static ushort GetId(Type type)
    {
        using var locker = LockObj.EnterScope();

        if (ComponentTypes.TryGetValue(type, out var exists)) return exists;

        var componentTypeId = _nextId++;
        ComponentTypes[type] = componentTypeId;

        return componentTypeId;
    }

    /// <summary>
    /// Получает тип компонента по его идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор типа компонента</param>
    /// <returns>Тип компонента</returns>
    /// <exception cref="Exception">Выбрасывается, если компонент с указанным идентификатором не найден</exception>
    public static Type GetType(ushort id)
    {
        using var locker = LockObj.EnterScope();

        foreach (var (type, existsId) in ComponentTypes)
        {
            if (existsId == id) return type;
        }

        ActorError.ComponentTypeNotFound(id);
        return null;
    }
}

internal static class ActorComponentType<T>
    where T : struct, IActorComponent
{
    /// <summary>
    /// Уникальный идентификатор типа компонента T.
    /// </summary>
    public static readonly ushort Id = ActorComponentType.GetId(typeof(T));

    /// <summary>
    /// Реализует ли этот компонент интерфейс <see cref="IViewComponent"/>
    /// </summary>
    public static readonly bool IsView = typeof(T).IsAssignableTo(typeof(IViewComponent));
}