using Hexecsm.Components.Messages;

namespace Hexecsm.Worlds;

public sealed partial class World
{
    public void Handle(ComponentAdded message)
    {
        ComponentAddedHandler(message.ActorId, message.ComponentTypeId);
    }

    public void Handle(ComponentRemoved message)
    {
        ComponentRemovedHandler(message.ActorId, message.ComponentTypeId);
    }
}
