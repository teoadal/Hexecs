namespace Hexecs.Pipelines.Messages;

internal static class MessageType
{
    private static readonly Dictionary<Type, ushort> Types = new(128, ReferenceComparer<Type>.Instance);
    private static readonly Lock LockObj = new();
    private static ushort _nextId;

    public static uint GetId(Type type)
    {
        using var locker = LockObj.EnterScope();

        if (Types.TryGetValue(type, out var exists)) return exists;

        var messageId = _nextId++;
        Types[type] = messageId;

        return messageId;
    }

    public static Type GetType(ushort id)
    {
        using var locker = LockObj.EnterScope();
        foreach (var (type, existsId) in Types)
        {
            if (existsId == id) return type;
        }

        PipelineError.MessageTypeNotFound(id);
        return null;
    }
}

internal static class MessageType<T>
    where T : struct, IMessage
{
    public static readonly uint Id = MessageType.GetId(typeof(T));
}