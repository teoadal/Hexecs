namespace Hexecs.Pipelines.Commands;

internal static class CommandType
{
    private static readonly Dictionary<Type, ushort> Types = new(128, ReferenceComparer<Type>.Instance);
    private static readonly Lock LockObj = new();
    private static ushort _nextId;

    public static ushort GetId(Type type)
    {
        using var locker = LockObj.EnterScope();

        if (Types.TryGetValue(type, out var exists)) return exists;

        var commandId = _nextId++;
        Types[type] = commandId;

        return commandId;
    }

    public static Type GetType(ushort id)
    {
        using var locker = LockObj.EnterScope();

        foreach (var (type, existsId) in Types)
        {
            if (existsId == id) return type;
        }

        PipelineError.CommandTypeNotFound(id);
        return null;
    }
}

internal static class CommandType<T>
    where T : struct
{
    public static readonly ushort Id = CommandType.GetId(typeof(T));
}