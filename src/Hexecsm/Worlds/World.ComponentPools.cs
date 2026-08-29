using Hexecsm.Components;
using Hexecsm.Utils;

namespace Hexecsm.Worlds;

public sealed partial class World
{
    private IComponentPool?[] _componentPools = [];
    private readonly Lock _componentPoolLock = new Lock();

    [MethodImpl(MethodImplOptions.NoInlining)]
    private ComponentPool<T> CreateComponentPool<T>()
        where T : struct, IComponent
    {
        ushort componentTypeIdRaw = ComponentType<T>.IdRaw;

        using (_componentPoolLock.EnterScope())
        {
            // Повторная проверка под локом (Double-Check Locking)
            if (componentTypeIdRaw < _componentPools.Length)
            {
                IComponentPool? existsPool = _componentPools[componentTypeIdRaw];

                if (existsPool != null)
                {
                    return Unsafe.As<ComponentPool<T>>(existsPool);
                }
            }

            ArrayUtils.EnsureCapacity(ref _componentPools, componentTypeIdRaw + 1);
            ref IComponentPool? pool = ref _componentPools[componentTypeIdRaw];
            pool ??= new ComponentPool<T>(
                cloneHandler: null,
                disposeHandler: null,
                eventBus: _eventBus,
                initialCapacity: 256);

            return Unsafe.As<ComponentPool<T>>(pool);
        }
    }

    private ComponentPool<T>? GetComponentPool<T>()
        where T : struct, IComponent
    {
        ushort componentTypeIdRaw = ComponentType<T>.IdRaw;
        IComponentPool?[] pools = _componentPools;

        if (componentTypeIdRaw < pools.Length)
        {
            IComponentPool? pool = pools[componentTypeIdRaw];

            return pool == null
                ? null
                : Unsafe.As<ComponentPool<T>>(pool);
        }

        return null;
    }

    private ComponentPool<T> GetOrAddComponentPool<T>()
        where T : struct, IComponent
    {
        ushort componentTypeIdRaw = ComponentType<T>.IdRaw;
        IComponentPool?[] pools = _componentPools;

        if ((uint)componentTypeIdRaw < (uint)pools.Length)
        {
            IComponentPool? existsPool = pools[componentTypeIdRaw];

            if (existsPool != null)
            {
                return Unsafe.As<ComponentPool<T>>(existsPool);
            }
        }

        return CreateComponentPool<T>();
    }
}
