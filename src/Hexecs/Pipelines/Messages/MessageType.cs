namespace Hexecs.Pipelines.Messages;

internal static class MessageType
{
    private static readonly Dictionary<Type, ushort> Types = new(128, ReferenceComparer<Type>.Instance);
#if NET9_0_OR_GREATER
    private static readonly Lock LockObj = new();
#else
    private static readonly object LockObj = new();
#endif
    private static ushort _nextId;

    public static uint GetId(Type type)
    {
#if NET9_0_OR_GREATER
        using (LockObj.EnterScope())
#else
        lock (LockObj)
#endif
        {
            if (Types.TryGetValue(type, out var exists)) return exists;

            var messageId = _nextId++;
            Types[type] = messageId;

            return messageId;
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

            PipelineError.MessageTypeNotFound(id);
            return null;
        }
    }
}

internal static class MessageType<T>
    where T : struct, IMessage
{
    public static readonly uint Id = MessageType.GetId(typeof(T));
}