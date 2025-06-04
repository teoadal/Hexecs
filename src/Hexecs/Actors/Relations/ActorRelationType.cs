namespace Hexecs.Actors.Relations;

internal static class ActorRelationType
{
    private static readonly Dictionary<Type, ushort> RelationTypes = new(128, ReferenceComparer<Type>.Instance);
    private static readonly Lock LockObj = new();
    private static ushort _nextId;

    public static ushort GetId(Type type)
    {
        using var locker = LockObj.EnterScope();
        
        if (RelationTypes.TryGetValue(type, out var exists)) return exists;

        var componentTypeId = _nextId++;
        RelationTypes[type] = componentTypeId;

        return componentTypeId;
    }

    public static Type GetType(uint id)
    {
        using var locker = LockObj.EnterScope();
        
        foreach (var (type, existsId) in RelationTypes)
        {
            if (existsId == id) return type;
        }

        ActorError.RelationTypeNotFound(id);
        return null;
    }
}

internal static class ActorRelationType<T>
    where T : struct
{
    public static readonly ushort Id = ActorRelationType.GetId(typeof(T));
}