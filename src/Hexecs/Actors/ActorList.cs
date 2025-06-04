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

    private uint[] _array;
    private bool _disposed;
    private int _length;
    private readonly ActorComponentPool<T> _pool;

    public ActorList(ActorContext context, int capacity = 8)
    {
        _array = capacity == 0
            ? []
            : ArrayPool<uint>.Shared.Rent(capacity);

        _length = 0;
        _pool = context.GetOrCreateComponentPool<T>();
        _pool.Removing += OnRemoved;

        context.Cleared += OnCleared;
    }

    private void OnCleared() => Clear();

    public void Add(in Actor<T> actor) => Add(actor.Id);

    public void Add(in ActorRef<T> actor) => Add(actor.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(uint actorId)
    {
        if (_disposed) ActorError.Disposed(typeof(ActorList<T>));

        ArrayUtils.Insert(ref _array, ArrayPool<uint>.Shared, _length, actorId);
        _length++;
    }

    public void Clear()
    {
        if (_array.Length > 0) ArrayPool<uint>.Shared.Return(_array);

        _array = [];
        _length = 0;
    }

    public bool Contains(in Actor<T> actor) => Contains(actor.Id);

    public bool Contains(in ActorRef<T> actor) => Contains(actor.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(uint actorId) => Array.IndexOf(_array, actorId, 0, _length) != -1;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        Clear();

        Context.Cleared -= OnCleared;
        _pool.Removing -= OnRemoved;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator() => _length == 0
        ? Enumerator.Empty
        : new Enumerator(this);

    public bool Remove(in Actor<T> actor) => Remove(actor.Id);

    public bool Remove(in ActorRef<T> actor) => Remove(actor.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(uint actorId)
    {
        var index = Array.IndexOf(_array, actorId, 0, _length);
        if (index == -1) return false;

        ArrayUtils.Cut(_array, index);
        _length--;

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Span<uint> AsSpan() => _length == 0
        ? Span<uint>.Empty
        : _array.AsSpan(0, _length);

    private void OnRemoved(uint actorId) => Remove(actorId);
}