namespace Hexecs.Pipelines.Notifications;

internal static class NotificationType
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

            var notificationId = _nextId++;
            Types[type] = notificationId;

            return notificationId;
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

            PipelineError.NotificationTypeNotFound(id);
            return null;
        }
    }
}

internal static class NotificationType<T>
    where T : struct, INotification
{
    public static readonly ushort Id = NotificationType.GetId(typeof(T));
}