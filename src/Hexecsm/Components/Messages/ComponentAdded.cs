using Hexecsm.Events;

namespace Hexecsm.Components.Messages;

[method: SkipLocalsInit]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly struct ComponentAdded(ActorId actorId, ComponentTypeId componentTypeId) : IMessage
{
    public readonly ActorId ActorId = actorId;
    public readonly ComponentTypeId ComponentTypeId = componentTypeId;
}

[method: SkipLocalsInit]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly struct ComponentAdded<TComponent>(ActorId actorId, in TComponent component) : IMessage
    where TComponent : struct, IComponent
{
    public readonly ActorId ActorId = actorId;
    public readonly TComponent Component = component;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ComponentAdded(in ComponentAdded<TComponent> instance)
    {
        return new ComponentAdded(instance.ActorId, ComponentType<TComponent>.Id);
    }
}
