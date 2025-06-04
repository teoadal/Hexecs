using Hexecs.Actors.Components;

namespace Hexecs.Actors;

public sealed class ActorMap2D<T>
    where T : struct, IActorComponent
{
    public event Action<uint>? Added;
    public event Action? Cleared;
    public event Action<uint>? Removed;

    public readonly ActorContext Context;

    private readonly int _maxY;
    private readonly Func<T, Position2D> _positionExtractor;
    private readonly XLine[] _values;
    private readonly ActorComponentPool<T> _pool;

    private bool _disposed;

    public ActorMap2D(ActorContext context, Func<T, Position2D> positionExtractor, int maxX, int maxY)
    {
        Context = context;

        _maxY = maxY;
        _pool = context.GetOrCreateComponentPool<T>();
        _positionExtractor = positionExtractor;
        _values = new XLine[maxX];

        _pool.ComponentAdded += OnComponentAdded;
        _pool.ComponentRemoving += OnComponentRemoving;
        _pool.ComponentUpdating += OnComponentUpdating;

        context.Cleared += OnCleared;

        Update();
    }

    public bool Contains(int x, int y)
    {
        if (_disposed || x >= _values.Length) return false;

        ref readonly var yValues = ref _values[x].Array;
        if (yValues == null || yValues.Length >= y) return false;

        ref readonly var value = ref yValues[y];
        return value.Id != Actor.EmptyId;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in Position2D position) => Contains(position.X, position.Y);

    public void Dispose()
    {
        if (_disposed) return;

        OnCleared();

        _pool.ComponentAdded -= OnComponentAdded;
        _pool.ComponentRemoving -= OnComponentRemoving;
        _pool.ComponentUpdating -= OnComponentUpdating;

        Context.Cleared -= OnCleared;

        _disposed = true;
    }

    public ActorRef<T> GetActorRef(int x, int y)
    {
        if (!_disposed && x < _values.Length)
        {
            ref readonly var yValues = ref _values[x].Array;
            if (yValues != null && y < yValues.Length)
            {
                ref readonly var value = ref yValues[y];
                if (value.Id != Actor.EmptyId)
                {
                    return new ActorRef<T>(Context, value.Id, ref _pool.GetByIndex(value.Index));
                }
            }
        }

        ActorError.KeyNotFound();
        return ActorRef<T>.Empty;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ActorRef<T> GetActorRef(in Position2D position) => GetActorRef(position.X, position.Y);

    public bool TryGetActorRef(int x, int y, out ActorRef<T> actor)
    {
        if (!_disposed && x < _values.Length)
        {
            ref readonly var yValues = ref _values[x].Array;
            if (yValues != null && y < yValues.Length)
            {
                ref readonly var value = ref yValues[y];
                if (value.Id != Actor.EmptyId)
                {
                    actor = new ActorRef<T>(Context, value.Id, ref _pool.GetByIndex(value.Index));
                    return true;
                }
            }
        }

        actor = ActorRef<T>.Empty;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetActorRef(in Position2D position, out ActorRef<T> actor)
    {
        return TryGetActorRef(position.X, position.Y, out actor);
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

    public ActorRef<T> this[int x, int y]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetActorRef(x, y);
    }

    public ActorRef<T> this[in Position2D position]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetActorRef(position.X, position.Y);
    }

    private void OnCleared()
    {
        for (var i = 0; i < _values.Length; i++)
        {
            ref readonly var yValues = ref _values[i].Array;
            if (yValues == null) continue;

            foreach (ref var yValue in yValues.AsSpan())
            {
                yValue = Entry.Empty;
            }
        }

        Cleared?.Invoke();
    }

    private void OnComponentAdded(uint actorId, int index, ref T component)
    {
        var (x, y) = _positionExtractor(component);
        if (x >= _values.Length) return;

        ref var yValues = ref _values[x].Array;
        yValues ??= new Entry[_maxY];

        if (y >= yValues.Length) return;
        yValues[y] = new Entry(actorId, index);

        Added?.Invoke(actorId);
    }

    private void OnComponentRemoving(uint actorId, ref T component)
    {
        var (x, y) = _positionExtractor(component);
        if (x >= _values.Length) return;

        ref var yValues = ref _values[x].Array;
        yValues ??= new Entry[_maxY];

        if (y >= yValues.Length) return;
        yValues[y] = Entry.Empty;

        Removed?.Invoke(actorId);
    }

    private void OnComponentUpdating(uint actorId, ref T exists, in T expected)
    {
        var (xExists, yExists) = _positionExtractor(exists);

        if (xExists < _values.Length)
        {
            ref var yValues = ref _values[xExists].Array;
            if (yValues != null && yExists < yValues.Length)
            {
                yValues[yExists] = Entry.Empty;
            }
        }

        var (xExpected, yExpected) = _positionExtractor(expected);
        if (xExpected >= _values.Length) return;

        ref var yExpectedValues = ref _values[xExpected].Array;
        yExpectedValues ??= new Entry[_maxY];

        if (yExpected >= yExpectedValues.Length) return;
        yExpectedValues[yExpected] = Entry.Empty;
    }

    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    private struct XLine(Entry[] array)
    {
        public Entry[]? Array = array;
    }

    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly struct Entry(uint id, int index)
    {
        public static readonly Entry Empty = new(Actor.EmptyId, 0);

        public readonly uint Id = id;
        public readonly int Index = index;
    }
}