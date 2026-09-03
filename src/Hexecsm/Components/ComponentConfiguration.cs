using Hexecsm.Handlers;

namespace Hexecsm.Components;

public sealed class ComponentConfiguration<TComponent>
    where TComponent : struct, IComponent
{
    internal int? InitialCapacity { get; private set; }

    internal ComponentCloneHandler<TComponent>? CloneHandler { get; private set; }

    internal ComponentDisposeHandler<TComponent>? DisposeHandler { get; private set; }

    internal ComponentConfiguration()
    {
    }

    public ComponentConfiguration<TComponent> AddCloneHandler(ComponentCloneHandler<TComponent> cloneHandler)
    {
        CloneHandler = cloneHandler;

        return this;
    }

    public ComponentConfiguration<TComponent> AddDisposeHandler(ComponentDisposeHandler<TComponent> disposeHandler)
    {
        DisposeHandler = disposeHandler;

        return this;
    }

    public ComponentConfiguration<TComponent> WithInitialCapacity(int initialCapacity)
    {
        InitialCapacity = initialCapacity;

        return this;
    }
}
