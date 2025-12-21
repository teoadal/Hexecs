namespace Hexecs.Assets.Components;

internal static class AssetComponentType
{
    private static readonly Dictionary<Type, ushort> ComponentTypes = new(128, ReferenceComparer<Type>.Instance);
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
            if (ComponentTypes.TryGetValue(type, out var exists)) return exists;

            var componentTypeId = _nextId++;
            ComponentTypes[type] = componentTypeId;

            return componentTypeId;
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
            foreach (var (type, existsId) in ComponentTypes)
            {
                if (existsId == id) return type;
            }

            AssetError.ComponentTypeNotFound(id);
            return null;
        }
    }
}

internal static class AssetComponentType<T>
    where T : struct, IAssetComponent
{
    public static readonly ushort Id = AssetComponentType.GetId(typeof(T));
}