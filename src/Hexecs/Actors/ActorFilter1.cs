using Hexecs.Actors.Components;
using Hexecs.Actors.Delegates;

namespace Hexecs.Actors;

[DebuggerTypeProxy(typeof(ActorFilter<>.DebugProxy))]
[DebuggerDisplay("Length = {Length}")]
public sealed partial class ActorFilter<T1> : IActorFilter
    where T1 : struct, IActorComponent
{
    public event Action<uint>? Added;
    public event Action? Cleared;
    public event Action<uint>? Removed;

    public readonly ActorConstraint? Constraint;
    public readonly ActorContext Context;

    private readonly Queue<Operation> _postponedUpdates;
    private int _postponedReadersCount;
    private readonly Lock _postponedSyncLock;

    private readonly ActorComponentPool<T1> _pool1;

    private bool _disposed;

    internal ActorFilter(ActorContext context, ActorConstraint? constraint = null, int capacity = 16)
    {
        Constraint = constraint;
        Context = context;

        _sparsePages = new uint[16][];
        _dense = new uint[capacity];
        _values = new int[capacity];

        _postponedUpdates = new Queue<Operation>(capacity);
        _postponedReadersCount = 0;
        _postponedSyncLock = new Lock();

        if (constraint != null)
        {
            constraint.Added += OnAdded;
            constraint.Removing += OnRemoving;
        }

        _pool1 = context.GetOrCreateComponentPool<T1>();

        _pool1.ComponentAdded += OnAddedComponent1;
        _pool1.ComponentRemoving += OnRemovingComponent1;

        foreach (var actor in context)
        {
            OnAdded(actor.Id);
        }

        context.Cleared += OnCleared;
    }

    #region Contains

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(uint actorId) => ContainsEntry(actorId);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in ActorId actorId) => ContainsEntry(actorId.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in ActorId<T1> actorId) => ContainsEntry(actorId.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in Actor actor) => ContainsEntry(actor.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in Actor<T1> actor) => ContainsEntry(actor.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in ActorRef<T1> actor) => ContainsEntry(actor.Id);

    #endregion

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        
        ClearEntries();
        
        if (Constraint != null)
        {
            Constraint.Added -= OnAdded;
            Constraint.Removing -= OnRemoving;
        }

        _pool1.ComponentAdded -= OnAddedComponent1;
        _pool1.ComponentRemoving -= OnRemovingComponent1;

        Context.Cleared -= OnCleared;
    }

    public ActorRef<T1> GetRef(uint actorId)
    {
        ref var entry = ref GetEntryRef(actorId);
        if (Unsafe.IsNullRef(ref entry)) ActorError.NotFound(actorId);

        return new ActorRef<T1>(
            Context,
            actorId,
            ref _pool1.GetByIndex(entry));
    }

    public ActorRef<T1> GetRef(ActorPredicate<T1> predicate)
    {
        foreach (var actor in this)
        {
            if (predicate(in actor)) return actor;
        }

        ActorError.ApplicableNotFound();
        return ActorRef<T1>.Empty;
    }

    public Actor[] ToArray()
    {
        using (_postponedSyncLock.EnterScope())
        {
            Interlocked.Increment(ref _postponedReadersCount);
        }

        try
        {
            var count = _count;
            if (count == 0) return [];

            var actors = new Actor[count];
            var keys = _dense;
            var ctx = Context;

            for (var i = 0; i < count; i++)
            {
                actors[i] = new Actor(ctx, keys[i]);
            }

            return actors;
        }
        finally
        {
            ProcessPostponedUpdates();
        }
    }

    private void OnAdded(uint actorId)
    {
        var index1 = _pool1.TryGetIndex(actorId);
        if (index1 == -1) return;

        Add(actorId, index1);
    }

    private void OnAddedComponent1(uint actorId, int index1, ref T1 component)
    {
        Add(actorId, index1);
    }

    private void OnCleared()
    {
        ClearEntries();
        Cleared?.Invoke();
    }

    private void OnRemoving(uint actorId) => Remove(actorId);

    private void OnRemovingComponent1(uint actorId, ref T1 component) => Remove(actorId);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Add(uint actorId, int index1)
    {
        if (Constraint != null && !Constraint.Applicable(actorId)) return;

        if (Volatile.Read(ref _postponedReadersCount) == 0)
        {
            using var locker = _postponedSyncLock.EnterScope();
            if (Volatile.Read(ref _postponedReadersCount) == 0)
            {
                AddEntry(actorId, index1);
            }
        }
        else
        {
            _postponedUpdates.Enqueue(Operation.Add(actorId, index1));
        }
    }

    private void ProcessPostponedUpdates()
    {
        if (Interlocked.Decrement(ref _postponedReadersCount) > 0) return;

        var isClear = false;
        using (_postponedSyncLock.EnterScope())
        {
            if (Volatile.Read(ref _postponedReadersCount) > 0) return;

            while (_postponedUpdates.TryDequeue(out var operation))
            {
                if (operation.IsClear)
                {
                    ClearEntries();
                    _postponedUpdates.Clear();
                    isClear = true;
                }
                else if (operation.IsAdd)
                {
                    AddEntry(operation.Id, operation.Index1);
                }
                else
                {
                    RemoveEntry(operation.Id);
                }
            }
        }

        // Вызываем событие вне лока, чтобы избежать дедлоков
        if (isClear) Cleared?.Invoke();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Remove(uint actorId)
    {
        if (Volatile.Read(ref _postponedReadersCount) == 0)
        {
            using var locker = _postponedSyncLock.EnterScope();
            if (Volatile.Read(ref _postponedReadersCount) == 0)
            {
                RemoveEntry(actorId);
            }
        }
        else
        {
            _postponedUpdates.Enqueue(Operation.Remove(actorId));
        }
    }

    ActorContext IActorFilter.Context => Context;
    ActorConstraint? IActorFilter.Constraint => Constraint;
}