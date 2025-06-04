using Hexecs.Actors.Components;
using Hexecs.Actors.Delegates;

namespace Hexecs.Actors;

[DebuggerTypeProxy(typeof(ActorFilter<,,>.DebugProxy))]
[DebuggerDisplay("Length = {Length}")]
public sealed partial class ActorFilter<T1, T2, T3> : IActorFilter
    where T1 : struct, IActorComponent
    where T2 : struct, IActorComponent
    where T3 : struct, IActorComponent
{
    public event Action<uint>? Added;
    public event Action? Cleared;
    public event Action<uint>? Removed;

    public readonly ActorConstraint? Constraint;
    public readonly ActorContext Context;

    private readonly Queue<Operation> _postponedUpdates;
    private int _postponedReadersCount;

    private readonly ActorComponentPool<T1> _pool1;
    private readonly ActorComponentPool<T2> _pool2;
    private readonly ActorComponentPool<T3> _pool3;

    internal ActorFilter(ActorContext context, ActorConstraint? constraint = null, int capacity = 16)
    {
        Constraint = constraint;
        Context = context;

        _dictionary = new Dictionary<uint, Entry>(capacity);

        _postponedUpdates = new Queue<Operation>(capacity);
        _postponedReadersCount = 0;

        if (constraint != null)
        {
            constraint.Added += OnAdded;
            constraint.Removing += OnRemoving;
        }

        _pool1 = context.GetOrCreateComponentPool<T1>();
        _pool2 = context.GetOrCreateComponentPool<T2>();
        _pool3 = context.GetOrCreateComponentPool<T3>();

        _pool1.ComponentAdded += OnAddedComponent1;
        _pool1.ComponentRemoving += OnRemovingComponent1;
        _pool2.ComponentAdded += OnAddedComponent2;
        _pool2.ComponentRemoving += OnRemovingComponent2;
        _pool3.ComponentAdded += OnAddedComponent3;
        _pool3.ComponentRemoving += OnRemovingComponent3;

        foreach (var actor in context)
        {
            OnAdded(actor.Id);
        }

        context.Cleared += OnCleared;
    }

    #region Contains

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(uint actorId) => _dictionary.ContainsKey(actorId);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in ActorId actorId) => Contains(actorId.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in ActorId<T1> actorId) => Contains(actorId.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in ActorId<T2> actorId) => Contains(actorId.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in ActorId<T3> actorId) => Contains(actorId.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in Actor actor) => Contains(actor.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in Actor<T1> actor) => Contains(actor.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in Actor<T2> actor) => Contains(actor.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in Actor<T3> actor) => Contains(actor.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in ActorRef<T1, T2, T3> actor) => Contains(actor.Id);

    #endregion

    public ActorRef<T1, T2, T3> GetRef(uint actorId)
    {
        ref var entry = ref CollectionsMarshal.GetValueRefOrNullRef(_dictionary, actorId);
        if (Unsafe.IsNullRef(ref entry)) ActorError.NotFound(actorId);

        return new ActorRef<T1, T2, T3>(
            Context,
            actorId,
            ref _pool1.GetByIndex(entry.Index1),
            ref _pool2.GetByIndex(entry.Index2),
            ref _pool3.GetByIndex(entry.Index3));
    }

    public ActorRef<T1, T2, T3> GetRef(ActorPredicate<T1, T2, T3> predicate)
    {
        var context = Context;
        foreach (var (key, value) in _dictionary)
        {
            var actor = new ActorRef<T1, T2, T3>(
                context,
                key,
                ref _pool1.GetByIndex(value.Index1),
                ref _pool2.GetByIndex(value.Index2),
                ref _pool3.GetByIndex(value.Index3));

            if (predicate(in actor)) return actor;
        }

        ActorError.ApplicableNotFound();
        return ActorRef<T1, T2, T3>.Empty;
    }

    private void OnAdded(uint actorId)
    {
        var index1 = _pool1.TryGetIndex(actorId);
        if (index1 == -1) return;

        var index2 = _pool2.TryGetIndex(actorId);
        if (index2 == -1) return;

        var index3 = _pool3.TryGetIndex(actorId);
        if (index3 == -1) return;

        Add(actorId, index1, index2, index3);
    }

    private void OnAddedComponent1(uint actorId, int index1, in T1 component)
    {
        var index2 = _pool2.TryGetIndex(actorId);
        if (index2 == -1) return;

        var index3 = _pool3.TryGetIndex(actorId);
        if (index3 == -1) return;

        Add(actorId, index1, index2, index3);
    }

    private void OnAddedComponent2(uint actorId, int index2, in T2 component)
    {
        var index1 = _pool1.TryGetIndex(actorId);
        if (index1 == -1) return;

        var index3 = _pool3.TryGetIndex(actorId);
        if (index3 == -1) return;

        Add(actorId, index1, index2, index3);
    }

    private void OnAddedComponent3(uint actorId, int index3, in T3 component)
    {
        var index1 = _pool1.TryGetIndex(actorId);
        if (index1 == -1) return;

        var index2 = _pool2.TryGetIndex(actorId);
        if (index2 == -1) return;

        Add(actorId, index1, index2, index3);
    }

    private void OnCleared()
    {
        ClearEntries();
        Cleared?.Invoke();
    }

    private void OnRemoving(uint actorId) => Remove(actorId);

    private void OnRemovingComponent1(uint actorId, in T1 component) => Remove(actorId);

    private void OnRemovingComponent2(uint actorId, in T2 component) => Remove(actorId);

    private void OnRemovingComponent3(uint actorId, in T3 component) => Remove(actorId);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Add(uint actorId, int index1, int index2, int index3)
    {
        if (Constraint != null && !Constraint.Applicable(actorId)) return;

        if (_postponedReadersCount == 0) AddEntry(actorId, index1, index2, index3);
        else _postponedUpdates.Enqueue(Operation.Add(actorId, index1, index2, index3));
    }

    private void ProcessPostponedUpdates()
    {
        if (--_postponedReadersCount > 0) return;
        while (_postponedUpdates.TryDequeue(out var operation))
        {
            if (operation.IsAdd) AddEntry(operation.Id, operation.Index1, operation.Index2, operation.Index3);
            else RemoveEntry(operation.Id);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Remove(uint actorId)
    {
        if (_postponedReadersCount == 0) RemoveEntry(actorId);
        else _postponedUpdates.Enqueue(Operation.Remove(actorId));
    }

    ActorContext IActorFilter.Context => Context;
    ActorConstraint? IActorFilter.Constraint => Constraint;
}