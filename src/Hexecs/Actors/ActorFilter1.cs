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

    private readonly ActorComponentPool<T1> _pool1;

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
    public bool Contains(uint actorId) => _dictionary.ContainsKey(actorId);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in ActorId actorId) => Contains(actorId.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in ActorId<T1> actorId) => Contains(actorId.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in Actor actor) => Contains(actor.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in Actor<T1> actor) => Contains(actor.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in ActorRef<T1> actor) => Contains(actor.Id);

    #endregion

    public ActorRef<T1> GetRef(uint actorId)
    {
        ref var entry = ref CollectionsMarshal.GetValueRefOrNullRef(_dictionary, actorId);
        if (Unsafe.IsNullRef(ref entry)) ActorError.NotFound(actorId);

        return new ActorRef<T1>(
            Context,
            actorId,
            ref _pool1.GetByIndex(entry.Index1));
    }

    public ActorRef<T1> GetRef(ActorPredicate<T1> predicate)
    {
        var context = Context;
        foreach (var (key, value) in _dictionary)
        {
            var actor = new ActorRef<T1>(context, key, ref _pool1.GetByIndex(value.Index1));
            if (predicate(in actor)) return actor;
        }

        ActorError.ApplicableNotFound<T1>();
        return ActorRef<T1>.Empty;
    }

    private void OnAdded(uint actorId)
    {
        var index1 = _pool1.TryGetIndex(actorId);
        if (index1 == -1) return;

        Add(actorId, index1);
    }

    private void OnAddedComponent1(uint actorId, int index1, in T1 component)
    {
        Add(actorId, index1);
    }

    private void OnCleared()
    {
        ClearEntries();
        Cleared?.Invoke();
    }

    private void OnRemoving(uint actorId) => Remove(actorId);

    private void OnRemovingComponent1(uint actorId, in T1 component) => Remove(actorId);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Add(uint actorId, int index1)
    {
        if (Constraint != null && !Constraint.Applicable(actorId)) return;

        if (_postponedReadersCount == 0) AddEntry(actorId, index1);
        else _postponedUpdates.Enqueue(Operation.Add(actorId, index1));
    }

    private void ProcessPostponedUpdates()
    {
        if (--_postponedReadersCount > 0) return;
        while (_postponedUpdates.TryDequeue(out var operation))
        {
            if (operation.IsAdd) AddEntry(operation.Id, operation.Index1);
            else RemoveEntry(operation.Id);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Remove(uint actorId)
    {
        if (_postponedReadersCount == 0) RemoveEntry(actorId);
        else _postponedUpdates.Enqueue(Operation.Remove(actorId));
    }

    public Actor[] ToArray()
    {
        if (Length == 0) return [];

        var index = 0;
        var result = new Actor[Length];
        foreach (var reference in this)
        {
            result[index++] = reference;
        }

        return result;
    }

    ActorContext IActorFilter.Context => Context;
    ActorConstraint? IActorFilter.Constraint => Constraint;
}