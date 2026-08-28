using Hexecsm.Components;
using Hexecsm.Utils;

namespace Hexecsm;

public sealed partial class World
{
    private IComponentPool?[] _componentPools = [];
    private readonly Lock _componentPoolLock = new Lock();

    [MethodImpl(MethodImplOptions.NoInlining)]
    private ComponentPool<T> CreateComponentPool<T>(ushort id)
        where T : struct, IComponent
    {
        using (_componentPoolLock.EnterScope())
        {
            // Повторная проверка под локом (Double-Check Locking)
            if (id < _componentPools.Length)
            {
                IComponentPool? existsPool = _componentPools[id];

                if (existsPool != null)
                {
                    return Unsafe.As<ComponentPool<T>>(existsPool);
                }
            }

            ArrayUtils.EnsureCapacity(ref _componentPools, id);
            ref IComponentPool? pool = ref _componentPools[id];
            pool ??= new ComponentPool<T>(
                cloneHandler: null,
                disposeHandler: null,
                initialCapacity: 256);

            return Unsafe.As<ComponentPool<T>>(pool);
        }
    }

    private ComponentPool<T>? GetComponentPool<T>()
        where T : struct, IComponent
    {
        ushort id = ComponentType<T>.Id;
        IComponentPool?[] pools = _componentPools;

        if (id < pools.Length)
        {
            IComponentPool? pool = pools[id];

            return pool == null
                ? null
                : Unsafe.As<ComponentPool<T>>(pool);
        }

        return null;
    }

    private ComponentPool<T> GetOrAddComponentPool<T>()
        where T : struct, IComponent
    {
        ushort id = ComponentType<T>.Id;
        IComponentPool?[] pools = _componentPools;

        if ((uint)id < (uint)pools.Length)
        {
            IComponentPool? existsPool = pools[id];

            if (existsPool != null)
            {
                return Unsafe.As<ComponentPool<T>>(existsPool);
            }
        }

        return CreateComponentPool<T>(id);
    }
}
