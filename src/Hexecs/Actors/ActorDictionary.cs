using Hexecs.Actors.Components;

namespace Hexecs.Actors;

[DebuggerDisplay("Length = {Length}")]
public sealed class ActorDictionary<TKey, T1> : IDisposable
    where TKey : IEquatable<TKey>
    where T1 : struct, IActorComponent
{
    public readonly ActorContext Context;

    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _actors.Count;
    }

    private readonly Dictionary<TKey, uint> _actors;
    private readonly Func<T1, TKey> _keyExtractor;
    private readonly ActorComponentPool<T1> _pool;

    private bool _disposed;

    public ActorDictionary(
        ActorContext context,
        Func<T1, TKey> keyExtractor,
        IEqualityComparer<TKey>? comparer = null,
        int capacity = 16)
    {
        Context = context;

        _actors = comparer == null
            ? new Dictionary<TKey, uint>(capacity)
            : new Dictionary<TKey, uint>(capacity, comparer);

        _keyExtractor = keyExtractor;
        _pool = context.GetOrCreateComponentPool<T1>();

        _pool.ComponentRemoving += OnComponentRemoving;
        _pool.ComponentUpdating += OnComponentUpdating;

        context.Cleared += OnCleared;
    }

    public void Add(Actor<T1> actor)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _actors.Add(_keyExtractor(actor.Component1), actor.Id);
    }

    public bool ContainsKey(TKey key) => !_disposed && _actors.ContainsKey(key);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _actors.Clear();

        _pool.ComponentRemoving -= OnComponentRemoving;
        _pool.ComponentUpdating -= OnComponentUpdating;

        Context.Cleared -= OnCleared;
    }

    public void Fill(bool clearBefore = true)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (clearBefore) OnCleared();

        var poolEnumerator = _pool.GetEnumerator();
        while (poolEnumerator.MoveNext())
        {
            var actor = poolEnumerator.Current;
            _actors.Add(_keyExtractor(actor.Component1), actor.Id);
        }
    }

    public ActorRef<T1> GetActorRef(TKey key)
    {
        if (!_disposed && _actors.TryGetValue(key, out var entry))
        {
            return new ActorRef<T1>(Context, entry, ref _pool.Get(entry));
        }

        ActorError.KeyNotFound();
        return ActorRef<T1>.Empty;
    }

    public bool TryGetActorRef(TKey key, out ActorRef<T1> actor)
    {
        if (!_disposed && _actors.TryGetValue(key, out var entry))
        {
            actor = new ActorRef<T1>(Context, entry, ref _pool.Get(entry));
            return true;
        }

        actor = ActorRef<T1>.Empty;
        return false;
    }

    public ActorRef<T1> this[TKey key]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetActorRef(key);
    }

    private void OnCleared()
    {
        _actors.Clear();
    }

    private void OnComponentRemoving(uint actorId, ref T1 component)
    {
        _actors.Remove(_keyExtractor(component));
    }

    private void OnComponentUpdating(uint actorId, ref T1 exists, in T1 expected)
    {
        _actors.Remove(_keyExtractor(exists), out _);
        _actors.Add(_keyExtractor(expected), actorId);
    }
}