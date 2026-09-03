using System.Collections.Concurrent;

using Hexecsm.Components;
using Hexecsm.Events;
using Hexecsm.Filters;
using Hexecsm.Systems;
using Hexecsm.Threading;
using Hexecsm.Utils;
using Hexecsm.Worlds.Messages;

namespace Hexecsm.Worlds;

public sealed partial class World : IDisposable
{
    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _storage.Length;
    }

    public readonly Dice Dice;

    public IParallelWorker ParallelWorker
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _parallelWorker;
    }

    private readonly ComponentPoolManager _componentPools;

    private readonly EventBus _eventBus;
    private readonly ActorDictionary<Entry> _storage;

    private readonly ConcurrentQueue<ActorId> _freeIds = [];
    private uint _nextActorId;

    private readonly Dictionary<Type, IFilter> _filters = new Dictionary<Type, IFilter>(ReferenceComparer<Type>.Instance);
    private readonly Lock _filtersLock = new Lock();

    private readonly DefaultParallelWorker _parallelWorker;

    private int _cycle;
    private long _previousDraw;
    private long _previousUpdate;
    private readonly long _startTime;

    internal World(
        Dictionary<Type, object> componentConfigurations,
        int initialCapacity,
        int degreeOfParallelism)
    {
        Dice = new Dice();

        _parallelWorker = new DefaultParallelWorker(degreeOfParallelism);

        _eventBus = new EventBus();
        _componentPools = new ComponentPoolManager(componentConfigurations, _eventBus);
        _storage = new ActorDictionary<Entry>(initialCapacity);

        _clearingProducer = _eventBus.GetProducer<WorldClearing>();

        long time = Environment.TickCount64;
        _cycle = 0;
        _previousDraw = time;
        _previousUpdate = time;
        _startTime = time;

        _drawSystems = [];
        _updateSystems = [];
    }

    public void AddComponent<T>(ActorId actorId, in T component)
        where T : struct, IComponent
    {
        ComponentPool<T> componentPool = _componentPools.GetOrAdd<T>();
        componentPool.Add(actorId, in component);

        PostponeOperation(Operation.AddComponent(actorId, ComponentType<T>.Id));
    }

    public void Clear()
    {
        PostponeOperation(Operation.Clear());
    }

    public ActorId CreateActor()
    {
        if (!_freeIds.TryDequeue(out ActorId actorId))
        {
            actorId = ActorId.Unsafe(Interlocked.Increment(ref _nextActorId));
        }

        PostponeOperation(Operation.AddActor(actorId));

        return actorId;
    }

    public void DestroyActor(ActorId actorId)
    {
        PostponeOperation(Operation.DestroyActor(actorId));
    }

    public void Dispose()
    {
        ClearHandler();
        ParallelWorker.Dispose();

        foreach (IFilter filter in _filters.Values)
        {
            filter.Dispose();
        }

        _filters.Clear();

        foreach (IComponentPool? componentPool in _componentPools.GetAll())
        {
            componentPool?.Dispose();
        }

        _eventBus.Dispose(); // should be last for gracefully unsubscribe
    }

    public void Draw(TimeSpan elapsed, TimeSpan total)
    {
        Draw(new WorldTime(_cycle, elapsed, total));
    }

    public void Draw(WorldTime? time = null)
    {
        long now = Environment.TickCount64;

        WorldTime worldTime = time ?? new WorldTime(_cycle, now - _previousDraw, now - _startTime);

        try
        {
            foreach (IDrawSystem drawSystem in _drawSystems)
            {
                if (drawSystem.Enabled)
                {
                    drawSystem.Draw(in worldTime);
                }
            }
        }
        finally
        {
            _previousDraw = now;
        }
    }

    public ref T GetComponent<T>(ActorId actorId)
        where T : struct, IComponent
    {
        ComponentPool<T>? componentPool = _componentPools.Get<T>();

        if (componentPool != null)
        {
            ref T component = ref componentPool.GetRef(actorId);

            if (!Unsafe.IsNullRef(ref component))
            {
                return ref component;
            }
        }

        ThrowComponentNotFound<T>(actorId);

        return ref Unsafe.NullRef<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Components<T> GetComponents<T>()
        where T : struct, IComponent
    {
        return new Components<T>(_componentPools.GetOrAdd<T>());
    }

    public Filter<T1> GetFilter<T1>()
        where T1 : struct, IComponent
    {
        Type key = typeof(Filter<T1>);

        using (_filtersLock.EnterScope())
        {
            if (_filters.TryGetValue(key, out IFilter? existsFilter))
            {
                return (Filter<T1>)existsFilter;
            }

            var filter = new Filter<T1>(
                componentPool1: _componentPools.GetOrAdd<T1>(),
                _eventBus);

            _filters[key] = filter;

            return filter;
        }
    }

    public Filter<T1, T2> GetFilter<T1, T2>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
    {
        Type key = typeof(Filter<T1, T2>);

        using (_filtersLock.EnterScope())
        {
            if (_filters.TryGetValue(key, out IFilter? existsFilter))
            {
                return (Filter<T1, T2>)existsFilter;
            }

            var filter = new Filter<T1, T2>(
                componentPool1: _componentPools.GetOrAdd<T1>(),
                componentPool2: _componentPools.GetOrAdd<T2>(),
                _eventBus);

            _filters[key] = filter;

            return filter;
        }
    }

    public Filter<T1, T2, T3> GetFilter<T1, T2, T3>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
    {
        Type key = typeof(Filter<T1, T2, T3>);

        using (_filtersLock.EnterScope())
        {
            if (_filters.TryGetValue(key, out IFilter? existsFilter))
            {
                return (Filter<T1, T2, T3>)existsFilter;
            }

            var filter = new Filter<T1, T2, T3>(
                componentPool1: _componentPools.GetOrAdd<T1>(),
                componentPool2: _componentPools.GetOrAdd<T2>(),
                componentPool3: _componentPools.GetOrAdd<T3>(),
                _eventBus);

            _filters[key] = filter;

            return filter;
        }
    }

    public bool HasComponent<T>(ActorId actorId)
        where T : struct, IComponent
    {
        ComponentPool<T>? componentPool = _componentPools.Get<T>();

        return componentPool != null && componentPool.Contains(actorId);
    }

    public bool IsAlive(ActorId actorId)
    {
        return _storage.Contains(actorId);
    }

    public void RemoveComponent<T>(ActorId actorId)
        where T : struct, IComponent
    {
        ComponentPool<T>? componentPool = _componentPools.Get<T>();

        if (componentPool != null)
        {
            componentPool.Remove(actorId);

            PostponeOperation(Operation.RemoveComponent(actorId, ComponentType<T>.Id));

            return;
        }

        ThrowComponentNotFound<T>(actorId);
    }

    public bool RemoveComponent<T>(ActorId actorId, out T component)
        where T : struct, IComponent
    {
        ComponentPool<T>? componentPool = _componentPools.Get<T>();

        if (componentPool != null)
        {
            PostponeOperation(Operation.RemoveComponent(actorId, ComponentType<T>.Id));

            return componentPool.Remove(actorId, out component);
        }

        ThrowComponentNotFound<T>(actorId);
        component = default;

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update(TimeSpan elapsed, TimeSpan total)
    {
        Update(new WorldTime(_cycle, elapsed, total));
    }

    public void Update(WorldTime? time = null)
    {
        long now = Environment.TickCount64;
        _cycle++;

        WorldTime worldTime = time ?? new WorldTime(_cycle, now - _previousUpdate, now - _startTime);

        try
        {
            // 1. Systems update
            foreach (IUpdateSystem updateSystem in _updateSystems)
            {
                updateSystem.Update(in worldTime);
            }

            // 2. Actor operations (add/remove)
            ProcessPostponedOperations();

            // 3. Component operations (add/remove/update_with_notification)
            foreach (IComponentPool? componentPool in _componentPools.GetAll())
            {
                componentPool?.ProcessPostponedOperations();
            }
        }
        finally
        {
            _previousUpdate = now;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ComponentPool<T1> GetOrAddComponentPool<T1>()
        where T1 : struct, IComponent
    {
        return _componentPools.GetOrAdd<T1>();
    }
}
