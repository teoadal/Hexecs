using Hexecsm.Accessors;
using Hexecsm.Components;
using Hexecsm.Components.Messages;
using Hexecsm.Events;
using Hexecsm.Utils;
using Hexecsm.Worlds.Messages;

namespace Hexecsm.Filters;

public sealed partial class Filter<T1> : IFilter
    where T1 : struct, IComponent
{
    public KeyAccessor Keys
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _hashSet.Keys;
    }

    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _hashSet.Length;
    }

    private readonly ComponentPool<T1> _componentPool1;
    private readonly EventBus _eventBus;
    private readonly ActorHashSet _hashSet;

    internal Filter(
        ComponentPool<T1> componentPool1,
        EventBus eventBus)
    {
        _componentPool1 = componentPool1;
        _eventBus = eventBus;

        _eventBus.Subscribe(_component1AddedConsumer = Handle);
        _eventBus.Subscribe(_component1AddedRemovedConsumer = Handle);
        _eventBus.Subscribe(_worldClearingConsumer = Handle);

        _hashSet = new ActorHashSet(128);

        Init();
    }

    public void Dispose()
    {
        _hashSet.Clear();

        _eventBus.Unsubscribe(_component1AddedConsumer);
        _eventBus.Unsubscribe(_component1AddedRemovedConsumer);
        _eventBus.Unsubscribe(_worldClearingConsumer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator()
    {
        return new Enumerator(
            _hashSet.Keys.AsReadOnlySpan(),
            _componentPool1.Values);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public KeyAccessor GetKeys(int start, int length)
    {
        return _hashSet.GetKeys(start, length);
    }

    private void Init()
    {
        foreach (ActorId actorId in _componentPool1.Keys.AsReadOnlySpan())
        {
            _hashSet.TryAdd(actorId);
        }
    }
}
