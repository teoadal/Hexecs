namespace Hexecs.Pipelines.Commands;

internal static class CommandType
{
    private static readonly Dictionary<Type, ushort> Types = new(128, ReferenceComparer<Type>.Instance);
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
            if (Types.TryGetValue(type, out var exists)) return exists;

            var commandId = _nextId++;
            Types[type] = commandId;

            return commandId;
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
            foreach (var (type, existsId) in Types)
            {
                if (existsId == id) return type;
            }

            PipelineError.CommandTypeNotFound(id);
            return null;
        }
    }
}

internal static class CommandType<T>
    where T : struct
{
    public static readonly ushort Id = CommandType.GetId(typeof(T));
}