using Hexecs.Actors.Components;

namespace Hexecs.Actors;

[DebuggerDisplay("Length = {Length}")]
public sealed class ActorDictionary<TKey, T> : IDisposable
    where TKey : struct, IEquatable<TKey>
    where T : struct, IActorComponent
{
    public event Action<uint>? Added;
    public event Action? Cleared;
    public event Action<uint>? Removed;

    public readonly ActorContext Context;

    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _actors.Count;
    }

    private readonly Dictionary<TKey, Entry> _actors;
    private readonly Func<T, TKey> _keyExtractor;
    private readonly ActorComponentPool<T> _pool;

    private bool _disposed;

    public ActorDictionary(
        ActorContext context,
        Func<T, TKey> keyExtractor,
        IEqualityComparer<TKey>? comparer = null,
        int capacity = 16)
    {
        Context = context;

        _actors = comparer == null
            ? new Dictionary<TKey, Entry>(capacity)
            : new Dictionary<TKey, Entry>(capacity, comparer);

        _keyExtractor = keyExtractor;
        _pool = context.GetOrCreateComponentPool<T>();

        _pool.ComponentAdded += OnComponentAdded;
        _pool.ComponentRemoving += OnComponentRemoving;
        _pool.ComponentUpdating += OnComponentUpdating;

        context.Cleared += OnCleared;

        _actors.EnsureCapacity(_pool.Length);

        Update();
    }

    public bool ContainsKey(TKey key) => !_disposed && _actors.ContainsKey(key);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _actors.Clear();

        _pool.ComponentAdded -= OnComponentAdded;
        _pool.ComponentRemoving -= OnComponentRemoving;
        _pool.ComponentUpdating -= OnComponentUpdating;

        Context.Cleared -= OnCleared;
    }

    public ActorRef<T> GetActorRef(TKey key)
    {
        if (!_disposed && _actors.TryGetValue(key, out var entry))
        {
            return new ActorRef<T>(Context, entry.Id, ref _pool.GetByIndex(entry.Index));
        }

        ActorError.KeyNotFound();
        return ActorRef<T>.Empty;
    }

    public bool TryGetActorRef(TKey key, out ActorRef<T> actor)
    {
        if (!_disposed && _actors.TryGetValue(key, out var entry))
        {
            actor = new ActorRef<T>(Context, entry.Id, ref _pool.GetByIndex(entry.Index));
            return true;
        }

        actor = ActorRef<T>.Empty;
        return false;
    }

    public void Update(bool clearBefore = true)
    {
        if (clearBefore) OnCleared();

        var poolEnumerator = _pool.GetEnumerator();
        while (poolEnumerator.MoveNext())
        {
            var actor = poolEnumerator.Current;
            OnComponentAdded(actor.Id, poolEnumerator.Index, ref actor.Component1);
        }
    }

    public ActorRef<T> this[TKey key]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetActorRef(key);
    }

    private void OnCleared()
    {
        _actors.Clear();
        Cleared?.Invoke();
    }

    private void OnComponentAdded(uint actorId, int index, ref T component)
    {
        _actors.Add(_keyExtractor(component), new Entry(actorId, index));
        Added?.Invoke(actorId);
    }

    private void OnComponentRemoving(uint actorId, ref T component)
    {
        if (_actors.Remove(_keyExtractor(component)))
        {
            Removed?.Invoke(actorId);
        }
    }

    private void OnComponentUpdating(uint actorId, ref T exists, in T expected)
    {
        _actors.Remove(_keyExtractor(exists), out var entry);
        _actors.Add(_keyExtractor(expected), new Entry(actorId, entry.Index));
    }

    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly struct Entry(uint id, int index)
    {
        public readonly uint Id = id;
        public readonly int Index = index;
    }
}