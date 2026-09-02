using Hexecsm.Utils;

namespace Hexecsm.Components;

internal static class ComponentType
{
    private static readonly Dictionary<Type, ComponentTypeId> ComponentTypeIds = new Dictionary<Type, ComponentTypeId>(ReferenceComparer<Type>.Instance);
    private static readonly Dictionary<ComponentTypeId, Type> ComponentIdTypes = [];
    private static readonly Lock Locker = new Lock();

    private static ushort NextTypeId = 0;

    public static ComponentTypeId GetId<T>()
        where T : struct, IComponent
    {
        return ComponentType<T>.Id;
    }

    public static ushort GetIdRaw<T>()
        where T : struct, IComponent
    {
        return ComponentType<T>.IdRaw;
    }

    public static ComponentTypeId GetId(Type componentType)
    {
        using (Locker.EnterScope())
        {
            if (ComponentTypeIds.TryGetValue(componentType, out ComponentTypeId existsTypeId))
            {
                return existsTypeId;
            }

            ComponentTypeId typeId = ComponentTypeId.Unsafe(++NextTypeId);

            ComponentTypeIds[componentType] = typeId;
            ComponentIdTypes[typeId] = componentType;

            return typeId;
        }
    }

    public static Type GetType(ComponentTypeId typeId)
    {
        using (Locker.EnterScope())
        {
            if (ComponentIdTypes.TryGetValue(typeId, out Type? existsType))
            {
                return existsType;
            }
        }

        Throw($"Type with id '{typeId.Value}' isn't registered");

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
    public static readonly ComponentTypeId Id = ComponentType.GetId(typeof(T));

    public static ushort IdRaw
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Id.Value;
    }
}
