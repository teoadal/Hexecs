using Hexecsm.Accessors;
using Hexecsm.Components;
using Hexecsm.Events;
using Hexecsm.Utils;
using Hexecsm.Worlds.Messages;

namespace Hexecsm.Filters;

public sealed partial class Filter<T1, T2> : IFilter, IConsumer<WorldClearing>
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
        ComponentPool<T2> componentPool2,
        EventBus eventBus)
    {
        _componentPool1 = componentPool1;
        _componentPool2 = componentPool2;

        _consumer1 = new Consumer1(eventBus, this);
        _consumer2 = new Consumer2(eventBus, this);

        _hashSet = new ActorHashSet(128);
    }

    public void Dispose()
    {
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator()
    {
        return new Enumerator(
            _hashSet.Keys.AsReadOnlySpan(),
            _componentPool1.Values,
            _componentPool2.Values);
    }
}
