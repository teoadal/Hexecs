using Hexecsm.Accessors;
using Hexecsm.Components;
using Hexecsm.Components.Messages;
using Hexecsm.Events;
using Hexecsm.Utils;
using Hexecsm.Worlds.Messages;

namespace Hexecsm.Filters;

public sealed partial class Filter<T1, T2, T3, T4>
    : IFilter,
        IConsumer<ComponentAdded<T1>>,
        IConsumer<ComponentRemoved<T1>>,
        IConsumer<WorldClearing>
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
    where T4 : struct, IComponent
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
    private readonly ComponentPool<T3> _componentPool3;
    private readonly ComponentPool<T4> _componentPool4;
    private readonly EventBus _eventBus;
    private readonly ActorHashSet _hashSet;

    internal Filter(
        ComponentPool<T1> componentPool1,
        ComponentPool<T2> componentPool2,
        ComponentPool<T3> componentPool3,
        ComponentPool<T4> componentPool4,
        EventBus eventBus)
    {
        _componentPool1 = componentPool1;
        _componentPool2 = componentPool2;
        _componentPool3 = componentPool3;
        _componentPool4 = componentPool4;

        _eventBus = eventBus;
        _eventBus.Subscribe<ComponentAdded<T1>>(this);
        _eventBus.Subscribe<ComponentRemoved<T1>>(this);

        _consumer2 = new Consumer<T2>(eventBus, this);
        _consumer3 = new Consumer<T3>(eventBus, this);
        _consumer4 = new Consumer<T4>(eventBus, this);

        _hashSet = new ActorHashSet(128);

        Init();
    }

    public void Dispose()
    {
        _hashSet.Clear();

        _eventBus.Unsubscribe<ComponentAdded<T1>>(this);
        _eventBus.Unsubscribe<ComponentRemoved<T1>>(this);

        _consumer2.Dispose();
        _consumer3.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator()
    {
        return new Enumerator(
            _hashSet.Keys.AsReadOnlySpan(),
            _componentPool1.Values,
            _componentPool2.Values,
            _componentPool3.Values);
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
            if (_componentPool2.Contains(actorId) && _componentPool3.Contains(actorId) && _componentPool4.Contains(actorId))
            {
                _hashSet.TryAdd(actorId);
            }
        }
    }
}
