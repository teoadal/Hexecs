using Hexecsm.Events;
using Hexecsm.Worlds.Messages;

namespace Hexecsm.Worlds;

public sealed partial class World
{
    private IProducer<WorldClearing> _clearingProducer;
}
