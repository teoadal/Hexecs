using Hexecsm.Components;
using Hexecsm.Utils;

namespace Hexecsm;

public sealed partial class World(int initialCapacity)
{
    private readonly ActorDictionary<Entry> _storage = new ActorDictionary<Entry>(128);

    public void Clear()
    {
        PostponeOperation(Operation.Clear());
    }

    public void Start()
    {
    }

    public void Stop()
    {
    }

    public void Update()
    {
        ProcessPostponedOperations();

        foreach (IComponentPool? componentPool in _componentPools)
        {
            componentPool?.ProcessPostponedOperations();
        }
    }
}
