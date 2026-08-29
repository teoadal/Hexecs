using Hexecsm.Components;
using Hexecsm.Components.Messages;
using Hexecsm.Events;
using Hexecsm.Utils;
using Hexecsm.Worlds.Messages;

namespace Hexecsm.Worlds;

public sealed partial class World : IConsumer<ComponentAdded>, IConsumer<ComponentRemoved>
{
    private readonly EventBus _eventBus;
    private readonly ActorDictionary<Entry> _storage;

    public World(int initialCapacity)
    {
        _eventBus = new EventBus();
        _storage = new ActorDictionary<Entry>(initialCapacity);

        _clearingProducer = _eventBus.GetProducer<WorldClearing>();
    }

    public void Clear()
    {
        PostponeOperation(Operation.Clear());
    }

    public void Update()
    {
        // 1. Actor operations (add/remove)

        ProcessPostponedOperations();

        // 2. Component operations (add/remove/update_with_notification)
        foreach (IComponentPool? componentPool in _componentPools)
        {
            componentPool?.ProcessPostponedOperations();
        }
    }
}
