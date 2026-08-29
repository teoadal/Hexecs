using Hexecsm.Accessors;
using Hexecsm.Components;
using Hexecsm.Utils;

namespace Hexecsm.Filters;

public sealed partial class Filter<T1, T2>
    : IComponentAddedListener, IComponentRemovedListener, IWorldClearingListener, IDisposable
    where T1 : struct, IComponent
    where T2 : struct, IComponent
{
    public KeyAccessor Keys
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _hashSet.Keys;
    }

    private readonly ComponentPool<T1> _componentPool1;
    private readonly ComponentPool<T2> _componentPool2;
    private readonly ActorHashSet _hashSet;

    internal Filter(
        ComponentPool<T1> componentPool1,
        ComponentPool<T2> componentPool2)
    {
        _componentPool1 = componentPool1;
        _componentPool2 = componentPool2;

        _hashSet = new ActorHashSet(128);

        componentPool1.SubscribeAdded(this);
        componentPool1.SubscribeRemoved(this);

        componentPool2.SubscribeAdded(this);
        componentPool2.SubscribeRemoved(this);
    }

    public void Dispose()
    {
        _componentPool1.UnsubscribeAdded(this);
        _componentPool1.UnsubscribeRemoved(this);

        _componentPool2.UnsubscribeAdded(this);
        _componentPool2.UnsubscribeRemoved(this);

        _hashSet.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueAccessor<T1> GetComponents1()
    {
        return _componentPool1.Values;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueAccessor<T2> GetComponents2()
    {
        return _componentPool2.Values;
    }
}
