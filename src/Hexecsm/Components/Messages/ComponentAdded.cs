using Hexecsm.Events;

namespace Hexecsm.Components.Messages;

[method: SkipLocalsInit]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly struct ComponentAdded<TComponent>(ActorId actorId, in TComponent component) : IMessage
    where TComponent : struct, IComponent
{
    public readonly ActorId ActorId = actorId;
    public readonly TComponent Component = component;
}
