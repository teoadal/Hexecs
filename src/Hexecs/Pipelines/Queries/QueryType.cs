namespace Hexecs.Pipelines.Queries;

internal static class QueryType
{
    private static readonly Dictionary<Type, ushort> Types = new Dictionary<Type, ushort>(128, ReferenceComparer<Type>.Instance);
#if NET9_0_OR_GREATER
    private static readonly Lock LockObj = new Lock();
#else
    private static readonly object LockObj = new object();
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
            if (Types.TryGetValue(type, out ushort exists))
            {
                return exists;
            }

            ushort queryId = _nextId++;
            Types[type] = queryId;

            return queryId;
        }
    }

    public static Type GetType(ushort id)
    {
#if NET9_0_OR_GREATER
        using (LockObj.EnterScope())
#else
        lock (LockObj)
#endif
        {
            foreach ((Type type, ushort existsId) in Types)
            {
                if (existsId == id)
                {
                    return type;
                }
            }

            PipelineError.QueryTypeNotFound(id);
            return null;
        }
    }
}

internal static class QueryType<T>
    where T : struct
{
    public static readonly ushort Id = QueryType.GetId(typeof(T));
}