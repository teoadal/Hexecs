using Hexecsm.Accessors;
using Hexecsm.Handlers;
using Hexecsm.Utils;

namespace Hexecsm.Components;

internal sealed partial class ComponentPool<T>(
    ComponentCloneHandler<T>? cloneHandler,
    ComponentDisposeHandler<T>? disposeHandler,
    int initialCapacity) : IComponentPool
    where T : struct, IComponent
{
    public readonly ComponentTypeId ComponentTypeId = ComponentType<T>.Id;

    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _storage.Length;
    }

    public KeyAccessor Keys
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _storage.Keys;
    }

    public ValueAccessor<T> Values
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _storage.Values;
    }

    private readonly ActorDictionary<T> _storage = new ActorDictionary<T>(initialCapacity);

    private bool _disposed;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(ActorId actorId, in T component)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ComponentPool<T>));

        PostponeOperation(Operation.Add(actorId, in component));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ComponentPool<T>));

        PostponeOperation(Operation.Clear());
    }

    public void Clone(ActorId source, ActorId target)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ComponentPool<T>));

        ref T exists = ref _storage.TryGetRef(source);

        if (!Unsafe.IsNullRef(ref exists))
        {
            T clonedComponent = cloneHandler?.Invoke(source, target, in exists) ?? exists;
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

        _postponedOperations.Clear();

        ClearHandler();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public KeyValueAccessor<T> GetComponents()
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ComponentPool<T>));

        return _storage.GetAccessor();
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
