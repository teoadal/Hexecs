namespace Hexecs.Pipelines.Notifications;

internal static class NotificationType
{
    private static readonly Dictionary<Type, ushort> Types = new(128, ReferenceComparer<Type>.Instance);
    private static readonly Lock LockObj = new();
    private static ushort _nextId;

    public static ushort GetId(Type type)
    {
        using var locker = LockObj.EnterScope();

        if (Types.TryGetValue(type, out var exists)) return exists;

        var notificationId = _nextId++;
        Types[type] = notificationId;

        return notificationId;
    }

    public static Type GetType(ushort id)
    {
        using var locker = LockObj.EnterScope();

        foreach (var (type, existsId) in Types)
        {
            if (existsId == id) return type;
        }

        PipelineError.NotificationTypeNotFound(id);
        return null;
    }
}

internal static class NotificationType<T>
    where T : struct, INotification
{
    public static readonly ushort Id = NotificationType.GetId(typeof(T));
}