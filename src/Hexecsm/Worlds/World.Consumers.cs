using Hexecsm.Components.Messages;
using Hexecsm.Events;

namespace Hexecsm.Worlds;

public sealed partial class World
{
    void IConsumer<ComponentAdded>.Handle(ComponentAdded message)
    {
        ComponentAddedHandler(message.ActorId, message.ComponentTypeId);
    }

    void IConsumer<ComponentRemoved>.Handle(ComponentRemoved message)
    {
        ComponentRemovedHandler(message.ActorId, message.ComponentTypeId);
    }
}
