using Hexecsm.Events;
using Hexecsm.Worlds.Messages;

namespace Hexecsm.Components;

internal sealed partial class ComponentPool<T>
{
    void IConsumer<WorldClearing>.Handle(WorldClearing message)
    {
        ClearHandler();
    }
}
