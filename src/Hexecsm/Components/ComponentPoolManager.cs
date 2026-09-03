using Hexecsm.Events;
using Hexecsm.Utils;

namespace Hexecsm.Components;

internal sealed class ComponentPoolManager : ThreadSafeManager<IComponentPool>
{
    private readonly Dictionary<Type, object> _componentConfigurations;
    private readonly EventBus _eventBus;

    public ComponentPoolManager(
        Dictionary<Type, object> componentConfigurations,
        EventBus eventBus) : base(128)
    {
        _componentConfigurations = componentConfigurations;
        _eventBus = eventBus;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public ComponentPool<T>? Get<T>()
        where T : struct, IComponent
    {
        IComponentPool? pool = GetItem(ComponentType<T>.IdRaw);

        return pool != null
            ? Unsafe.As<ComponentPool<T>>(pool)
            : null;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public ComponentPool<T> GetOrAdd<T>()
        where T : struct, IComponent
    {
        IComponentPool pool = GetOrAddItem(
            index: ComponentType<T>.IdRaw,
            factory: static ctx => ctx.CreateComponentPool<T>(),
            arg: this);

        return Unsafe.As<ComponentPool<T>>(pool);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IComponentPool GetUnsafe(ComponentTypeId componentTypeId)
    {
        return GetItemUnsafe(componentTypeId.Value);
    }

    private ComponentPool<T> CreateComponentPool<T>()
        where T : struct, IComponent
    {
        ComponentConfiguration<T>? configuration = _componentConfigurations.TryGetValue(typeof(T), out object? existsConfiguration)
            ? (ComponentConfiguration<T>?)existsConfiguration
            : null;

        return new ComponentPool<T>(configuration, _eventBus);
    }
}
