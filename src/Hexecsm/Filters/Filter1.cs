using Hexecsm.Accessors;
using Hexecsm.Components;
using Hexecsm.Components.Messages;
using Hexecsm.Events;
using Hexecsm.Utils;
using Hexecsm.Worlds.Messages;

namespace Hexecsm.Filters;

public sealed partial class Filter<T1>
    : IFilter,
        IConsumer<ComponentAdded<T1>>,
        IConsumer<ComponentRemoved<T1>>,
        IConsumer<WorldClearing>
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
        _eventBus.Subscribe<ComponentAdded<T1>>(this);
        _eventBus.Subscribe<ComponentRemoved<T1>>(this);

        _hashSet = new ActorHashSet(128);
    }

    public void Dispose()
    {
        _hashSet.Clear();

        _eventBus.Unsubscribe<ComponentAdded<T1>>(this);
        _eventBus.Unsubscribe<ComponentRemoved<T1>>(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator()
    {
        return new Enumerator(
            _hashSet.Keys.AsReadOnlySpan(),
            _componentPool1.Values);
    }
}
