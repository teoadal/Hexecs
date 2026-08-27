namespace Hexecs.Pipelines.Commands;

internal static class CommandType
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

            ushort commandId = _nextId++;
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
            foreach ((Type type, ushort existsId) in Types)
            {
                if (existsId == id)
                {
                    return type;
                }
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