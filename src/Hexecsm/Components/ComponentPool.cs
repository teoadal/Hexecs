using Hexecsm.Accessors;
using Hexecsm.Components.Messages;
using Hexecsm.Events;
using Hexecsm.Handlers;
using Hexecsm.Utils;

namespace Hexecsm.Components;

internal sealed partial class ComponentPool<T> : IComponentPool
    where T : struct, IComponent
{
    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _storage.Length;
    }

    private readonly ActorDictionary<T> _storage;

    private bool _disposed;
    private readonly ComponentCloneHandler<T>? _cloneHandler;
    private readonly ComponentDisposeHandler<T>? _disposeHandler;
    private readonly EventBus _eventBus;

    public ComponentPool(
        ComponentConfiguration<T>? configuration,
        EventBus eventBus)
    {
        _cloneHandler = configuration?.CloneHandler;
        _disposeHandler = configuration?.DisposeHandler;
        _eventBus = eventBus;
        _storage = new ActorDictionary<T>(configuration?.InitialCapacity ?? 128);

        _addedProducer = eventBus.GetProducer<ComponentAdded<T>>();
        _removedProducer = eventBus.GetProducer<ComponentRemoved<T>>();
        _updatingProducer = eventBus.GetProducer<ComponentUpdating<T>>();

        _eventBus.Subscribe(_worldClearingConsumer = Handle);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(ActorId actorId, in T component)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ComponentPool<T>));

        PostponeOperation(Operation.Add(actorId, in component));
    }

    public void Clone(ActorId source, ActorId target)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ComponentPool<T>));

        ref T exists = ref _storage.TryGetRef(source);

        if (!Unsafe.IsNullRef(ref exists))
        {
            T clonedComponent = _cloneHandler?.Invoke(source, target, in exists) ?? exists;
            PostponeOperation(Operation.Clone(target, in clonedComponent));

            return;
        }

        ThrowComponentNotFound(source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(ActorId actorId)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ComponentPool<T>));

        return _storage.Contains(actorId);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        ClearHandler();
        _eventBus.Unsubscribe(_worldClearingConsumer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public KeyAccessor GetKeys()
    {
        return _storage.Keys;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public KeyValueAccessor<T> GetKeyValues()
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ComponentPool<T>));

        return _storage.GetAccessor();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueAccessor<T> GetValues()
    {
        return _storage.Values;
    }

    public ref T GetRef(ActorId actorId)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ComponentPool<T>));

        ref T exists = ref _storage.TryGetRef(actorId);

        if (!Unsafe.IsNullRef(ref exists))
        {
            return ref exists;
        }

        ThrowComponentNotFound(actorId);

        return ref Unsafe.NullRef<T>();
    }

    public void Remove(ActorId actorId)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ComponentPool<T>));

        PostponeOperation(Operation.Remove(actorId));
    }

    public bool Remove(ActorId actorId, out T component)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ComponentPool<T>));

        ref T entry = ref _storage.TryGetRef(actorId);

        if (Unsafe.IsNullRef(ref entry))
        {
            component = default;

            return false;
        }

        PostponeOperation(Operation.Remove(actorId, in entry));

        component = entry;

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T TryGetRef(ActorId actorId)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ComponentPool<T>));

        return ref _storage.TryGetRef(actorId);
    }

    public void Update(ActorId actorId, in T component)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ComponentPool<T>));

        PostponeOperation(Operation.Update(actorId, in component));
    }
}
