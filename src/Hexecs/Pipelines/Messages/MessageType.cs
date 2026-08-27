namespace Hexecs.Pipelines.Messages;

internal static class MessageType
{
    private static readonly Dictionary<Type, ushort> Types = new Dictionary<Type, ushort>(128, ReferenceComparer<Type>.Instance);
#if NET9_0_OR_GREATER
    private static readonly Lock LockObj = new Lock();
#else
    private static readonly object LockObj = new object();
#endif
    private static ushort NextId;

    public static uint GetId(Type type)
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

            ushort messageId = NextId++;
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
            foreach ((Type type, ushort existsId) in Types)
            {
                if (existsId == id)
                {
                    return type;
                }
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
