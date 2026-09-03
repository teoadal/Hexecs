using Hexecsm.Components;
using Hexecsm.Utils;

namespace Hexecsm.Operations;

internal sealed partial class OperationQueue
{
    private sealed class ComponentBufferManager(ComponentPoolManager componentPools) : ThreadSafeManager<IComponentBuffer>(128)
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public ComponentBuffer<T>? Get<T>()
            where T : struct, IComponent
        {
            IComponentBuffer? pool = GetItem(ComponentType<T>.IdRaw);

            return pool != null
                ? Unsafe.As<ComponentBuffer<T>>(pool)
                : null;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public ComponentBuffer<T> GetOrAdd<T>()
            where T : struct, IComponent
        {
            IComponentBuffer pool = GetOrAddItem(
                index: ComponentType<T>.IdRaw,
                factory: static ctx => ctx.CreateComponentBuffer<T>(),
                arg: this);

            return Unsafe.As<ComponentBuffer<T>>(pool);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IComponentBuffer GetUnsafe(ComponentTypeId componentTypeId)
        {
            return GetItemUnsafe(componentTypeId.Value);
        }

        private ComponentBuffer<T> CreateComponentBuffer<T>()
            where T : struct, IComponent
        {
            return new ComponentBuffer<T>(128, componentPools.GetOrAdd<T>());
        }
    }
}
