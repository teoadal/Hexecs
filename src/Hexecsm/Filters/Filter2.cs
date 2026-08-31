using Hexecsm.Accessors;
using Hexecsm.Components;
using Hexecsm.Components.Messages;
using Hexecsm.Events;
using Hexecsm.Utils;
using Hexecsm.Worlds.Messages;

namespace Hexecsm.Filters;

public sealed partial class Filter<T1, T2>
    : IFilter,
        IConsumer<ComponentAdded<T1>>,
        IConsumer<ComponentRemoved<T1>>,
        IConsumer<WorldClearing>
    where T1 : struct, IComponent
    where T2 : struct, IComponent
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
    private readonly ComponentPool<T2> _componentPool2;
    private readonly EventBus _eventBus;
    private readonly ActorHashSet _hashSet;

    internal Filter(
        ComponentPool<T1> componentPool1,
        ComponentPool<T2> componentPool2,
        EventBus eventBus)
    {
        _componentPool1 = componentPool1;
        _componentPool2 = componentPool2;

        _eventBus = eventBus;
        _eventBus.Subscribe<ComponentAdded<T1>>(this);
        _eventBus.Subscribe<ComponentRemoved<T1>>(this);

        _consumer2 = new Consumer2(eventBus, this);

        _hashSet = new ActorHashSet(128);

        Init();
    }

    public void Dispose()
    {
        _hashSet.Clear();

        _eventBus.Unsubscribe<ComponentAdded<T1>>(this);
        _eventBus.Unsubscribe<ComponentRemoved<T1>>(this);

        _consumer2.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator()
    {
        return new Enumerator(
            _hashSet.Keys.AsReadOnlySpan(),
            _componentPool1.Values,
            _componentPool2.Values);
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
            if (_componentPool2.Contains(actorId))
            {
                _hashSet.TryAdd(actorId);
            }
        }
    }
}
