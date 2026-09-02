using Hexecsm.Events;
using Hexecsm.Worlds.Messages;

namespace Hexecsm.Components;

internal sealed partial class ComponentPool<T>
{
    private readonly ConsumerDelegate<WorldClearing> _worldClearingConsumer;

    private void Handle(in WorldClearing _)
    {
        ClearHandler();
    }
}
