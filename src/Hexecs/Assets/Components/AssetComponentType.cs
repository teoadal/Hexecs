namespace Hexecs.Assets.Components;

internal static class AssetComponentType
{
    private static readonly Dictionary<Type, ushort> ComponentTypes = new(128, ReferenceComparer<Type>.Instance);
    private static readonly Lock LockObj = new();
    private static ushort _nextId;

    public static ushort GetId(Type type)
    {
        using var locker = LockObj.EnterScope();
        
        if (ComponentTypes.TryGetValue(type, out var exists)) return exists;

        var componentTypeId = _nextId++;
        ComponentTypes[type] = componentTypeId;

        return componentTypeId;
    }

    public static Type GetType(ushort id)
    {
        using var locker = LockObj.EnterScope();
        
        foreach (var (type, existsId) in ComponentTypes)
        {
            if (existsId == id) return type;
        }

        AssetError.ComponentTypeNotFound(id);
        return null;
    }
}

internal static class AssetComponentType<T>
    where T : struct, IAssetComponent
{
    public static readonly ushort Id = AssetComponentType.GetId(typeof(T));
}