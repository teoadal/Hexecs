namespace Hexecs.Actors.Relations;

internal static class ActorRelationType
{
    private static readonly Dictionary<Type, ushort> RelationTypes = new Dictionary<Type, ushort>(128, ReferenceComparer<Type>.Instance);
#if NET9_0_OR_GREATER
    private static readonly Lock LockObj = new Lock();
#else
    private static readonly object LockObj = new object();
#endif
    private static ushort NextId;

    public static ushort GetId(Type type)
    {
#if NET9_0_OR_GREATER
        using (LockObj.EnterScope())
#else
        lock (LockObj)
#endif
        {
            if (RelationTypes.TryGetValue(type, out ushort exists))
            {
                return exists;
            }

            ushort componentTypeId = NextId++;
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
            foreach ((Type type, ushort existsId) in RelationTypes)
            {
                if (existsId == id)
                {
                    return type;
                }
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
