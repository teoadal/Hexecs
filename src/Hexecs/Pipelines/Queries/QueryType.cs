namespace Hexecs.Pipelines.Queries;

internal static class QueryType
{
    private static readonly Dictionary<Type, ushort> Types = new(128, ReferenceComparer<Type>.Instance);
    private static readonly Lock LockObj = new();
    private static ushort _nextId;

    public static ushort GetId(Type type)
    {
        using var locker = LockObj.EnterScope();
        if (Types.TryGetValue(type, out var exists)) return exists;

        var queryId = _nextId++;
        Types[type] = queryId;

        return queryId;
    }

    public static Type GetType(ushort id)
    {
        using var locker = LockObj.EnterScope();
        foreach (var (type, existsId) in Types)
        {
            if (existsId == id) return type;
        }

        PipelineError.QueryTypeNotFound(id);
        return null;
    }
}

internal static class QueryType<T>
    where T : struct
{
    public static readonly ushort Id = QueryType.GetId(typeof(T));
}