namespace Hexecs.Actors.Relations;

internal static class ActorRelationType
{
    private static readonly Dictionary<Type, ushort> RelationTypes = new(128, ReferenceComparer<Type>.Instance);
#if NET9_0_OR_GREATER
    private static readonly Lock LockObj = new();
#else
    private static readonly object LockObj = new();
#endif
    private static ushort _nextId;

    public static ushort GetId(Type type)
    {
#if NET9_0_OR_GREATER
        using (LockObj.EnterScope())
#else
        lock (LockObj)
#endif
        {
            if (RelationTypes.TryGetValue(type, out var exists)) return exists;

            var componentTypeId = _nextId++;
            RelationTypes[type] = componentTypeId;

            return componentTypeId;
        }
    }

    public static Type GetType(uint id)
    {
#if NET9_0_OR_GREATER
        using (LockObj.EnterScope())
#else
        lock (LockObj)
#endif
        {
            foreach (var (type, existsId) in RelationTypes)
            {
                if (existsId == id) return type;
            }

            ActorError.RelationTypeNotFound(id);
            return null;
        }
    }
}

internal static class ActorRelationType<T>
    where T : struct
{
    public static readonly ushort Id = ActorRelationType.GetId(typeof(T));
}