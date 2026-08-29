using Hexecsm.Events;

namespace Hexecsm.Components.Messages;

[method: SkipLocalsInit]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly struct ComponentUpdating<TComponent>(
    ActorId actorId,
    in TComponent exists,
    in TComponent expected) : IMessage
    where TComponent : struct, IComponent
{
    public readonly ActorId ActorId = actorId;
    public readonly TComponent Exists = exists;
    public readonly TComponent Expected = expected;
}
