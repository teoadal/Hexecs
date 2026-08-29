using Hexecsm.Events;

namespace Hexecsm.Components;

public struct ComponentAdded<TComponent> : IMessage
    where TComponent : struct, IComponent
{
    public readonly ActorId ActorId;
    public readonly TComponent Component;

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ComponentAdded(ActorId actorId, TComponent component)
    {
        ActorId = actorId;
        Component = component;
    }
}
