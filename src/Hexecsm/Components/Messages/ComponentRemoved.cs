using Hexecsm.Events;

namespace Hexecsm.Components.Messages;

[method: SkipLocalsInit]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly struct ComponentRemoved(ActorId actorId, ComponentTypeId componentTypeId) : IMessage
{
    public readonly ActorId ActorId = actorId;
    public readonly ComponentTypeId ComponentTypeId = componentTypeId;
}

[method: SkipLocalsInit]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly struct ComponentRemoved<TComponent>(ActorId actorId, in TComponent component) : IMessage
    where TComponent : struct, IComponent
{
    public readonly ActorId ActorId = actorId;
    public readonly TComponent Component = component;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ComponentRemoved(in ComponentRemoved<TComponent> instance)
    {
        return new ComponentRemoved(instance.ActorId, ComponentType<TComponent>.Id);
    }
}
