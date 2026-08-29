using Hexecsm.Events;
using Hexecsm.Worlds.Messages;

namespace Hexecsm.Worlds;

public sealed partial class World
{
    private readonly IProducer<WorldClearing> _clearingProducer;

    private void ProduceClearingEvent()
    {
        _clearingProducer.Produce(new WorldClearing());
    }
}
