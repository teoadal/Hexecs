namespace Hexecsm.Components;

internal static class ComponentType
{
    private static readonly Dictionary<Type, ushort> ComponentTypeIds = [];
    private static readonly Dictionary<ushort, Type> ComponentIdTypes = [];
    private static readonly Lock Locker = new Lock();

    private static ushort NextTypeId = 0;

    public static ushort GetTypeId(Type componentType)
    {
        using (Locker.EnterScope())
        {
            if (ComponentTypeIds.TryGetValue(componentType, out ushort existsTypeId))
            {
                return existsTypeId;
            }

            ushort typeId = NextTypeId++;

            ComponentTypeIds[componentType] = typeId;
            ComponentIdTypes[typeId] = componentType;

            return typeId;
        }
    }

    public static Type GetTypeFromId(ushort typeId)
    {
        using (Locker.EnterScope())
        {
            if (ComponentIdTypes.TryGetValue(typeId, out Type? existsType))
            {
                return existsType;
            }
        }

        Throw($"Type with id {typeId} isn't registered");

        return null!;
    }

    private static void Throw(string message)
    {
        throw new Exception(message);
    }
}

internal static class ComponentType<T>
    where T : struct, IComponent
{
    public static readonly ushort Id = ComponentType.GetTypeId(typeof(T));
}
