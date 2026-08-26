using Hexecs.Actors.Components;

namespace Hexecs.Actors;

[DebuggerDisplay("Length = {Length}")]
public sealed partial class ActorList<T> : IDisposable
    where T : struct, IActorComponent
{
    public ActorContext Context
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _pool.Context;
    }

    // ReSharper disable once ConvertToAutoPropertyWhenPossible
    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _length;
    }

    private readonly ArrayPool<ActorId> _arrayPool;

    private ActorId[] _array;
    private bool _disposed;
    private int _length;
    private readonly ActorComponentPool<T> _pool;

    public ActorList(
        ActorContext context,
        ArrayPool<ActorId>? arrayPool = null,
        int capacity = 8)
    {
        _arrayPool = arrayPool ?? ArrayPool<ActorId>.Shared;
        
        _array = capacity == 0
            ? []
            : _arrayPool.Rent(capacity);

        _length = 0;
        _pool = context.GetOrCreateComponentPool<T>();
        _pool.Removing += OnRemoved;

        context.Cleared += OnCleared;
    }

    public void Add(in ActorRef<T> actor) => Add(actor.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(ActorId actorId)
    {
        if (_disposed) ActorError.Disposed(typeof(ActorList<T>));

        ArrayUtils.Insert(ref _array, _arrayPool, _length, actorId);
        _length++;
    }

    public void Clear()
    {
        if (_array.Length > 0) _arrayPool.Return(_array);

        _array = [];
        _length = 0;
    }

    public bool Contains(in ActorRef<T> actor)
    {
        return Contains(actor.Id);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(ActorId actorId)
    {
        return Array.IndexOf(_array, actorId, 0, _length) != -1;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        Clear();

        Context.Cleared -= OnCleared;
        _pool.Removing -= OnRemoved;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator()
    {
        return _length == 0
            ? Enumerator.Empty
            : new Enumerator(this);
    }

    public bool Remove(in ActorRef<T> actor)
    {
        return Remove(actor.Id);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(ActorId actorId)
    {
        var index = Array.IndexOf(_array, actorId, 0, _length);
        if (index == -1) return false;

        ArrayUtils.Cut(_array, index);
        _length--;

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Span<ActorId> AsSpan() => _length == 0
        ? Span<ActorId>.Empty
        : _array.AsSpan(0, _length);

    private void OnCleared() => Clear();

    private void OnRemoved(ActorId actorId)
    {
        Remove(actorId);
    }
}