using Hexecs.Actors.Delegates;
using Hexecs.Actors.Serializations;

namespace Hexecs.Actors.Components;

[SuppressMessage("ReSharper", "InvertIf")]
[SuppressMessage("ReSharper", "StaticMemberInGenericType")]
[DebuggerDisplay("{Length}")]
internal sealed partial class ActorComponentPool<T> : IActorComponentPool
    where T : struct, IActorComponent
{
    public event Action<ActorId>? Added;
    public event Action<ActorId>? Removing;

    public event ActorComponentAdded<T>? ComponentAdded;
    public event ActorComponentRemoving<T>? ComponentRemoving;
    public event ActorComponentUpdating<T>? ComponentUpdating;

    public readonly ActorContext Context;

    public ushort Id
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ActorComponentType<T>.Id;
    }

    public Type Type
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => typeof(T);
    }

    private readonly ActorCloneHandler<T>? _cloneHandler;
    private readonly IActorComponentConverter<T>? _converter;
    private readonly ActorDisposeHandler<T>? _disposeHandler;

    public ActorComponentPool(ActorContext context, ActorComponentConfiguration<T> configuration)
    {
        Context = context;

        int capacity = HashHelper.GetPrime(configuration.Capacity ?? Math.Max(context.Length, 16));

        _sparse = new uint[capacity];
        _dense = new uint[capacity];
        _values = new T[capacity];

        _cloneHandler = configuration.CloneHandler;
        _converter = configuration.Converter;
        _disposeHandler = configuration.DisposeHandler;
    }

    public ref T Add(ActorId ownerId, in T component)
    {
        AddResult result = TryAddEntry(ownerId.Value);
        if (result.IsSuccess)
        {
            ref T componentRef = ref result.Component;
            componentRef = component;

            Added?.Invoke(ownerId);
            ComponentAdded?.Invoke(ownerId, ref componentRef);

            return ref componentRef;
        }

        ActorError.ComponentExists<T>(ownerId);
        return ref Unsafe.NullRef<T>();
    }

    public void Clear()
    {
        uint[] dense = _dense;
        uint[] sparse = _sparse;

        if (_disposeHandler != null)
        {
            foreach (ref T component in _values.AsSpan(0, _count))
            {
                _disposeHandler(ref component);
            }
        }

        // Очищаем только те индексы в sparse, которые реально используются
        for (var i = 0; i < _count; i++)
        {
            uint key = dense[i];
            sparse[key] = 0;
        }

        _count = 0;
    }

    public ref T Clone(ActorId ownerId, ActorId cloneId)
    {
        ref T ownerEntry = ref GetEntryRef(ownerId.Value);
        if (Unsafe.IsNullRef(ref ownerEntry))
        {
            ActorError.ComponentNotFound<T>(ownerId);
        }

        if (_cloneHandler == null)
        {
            return ref Add(cloneId, in ownerEntry);
        }

        T clone = _cloneHandler(in ownerEntry);
        return ref Add(cloneId, Unsafe.AsRef(in clone));
    }

    public ActorRef<T> First()
    {
        return _count > 0
            ? new ActorRef<T>(Context, new ActorId(_dense[0]), ref _values[0])
            : ActorRef<T>.Empty;
    }

    public ActorRef<T> First(ActorPredicate<T> predicate)
    {
        int count = _count;
        uint[] keys = _dense;
        T[] values = _values;
        ActorContext context = Context;

        for (var i = 0; i < count; i++)
        {
            var actor = new ActorRef<T>(context, new ActorId(keys[i]), ref values[i]);
            if (predicate(in actor))
            {
                return actor;
            }
        }

        return ActorRef<T>.Empty;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Get(ActorId ownerId)
    {
        ref T entry = ref GetEntryRef(ownerId.Value);
        if (!Unsafe.IsNullRef(ref entry))
        {
            return ref entry;
        }

        return ref ActorError.ComponentNotFound<T>(ownerId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ComponentsAccess<T> GetComponentAccess()
    {
        return new ComponentsAccess<T>(_sparse, _values.AsSpan(0, _count));
    }

    public ref T GetOrCreate(ActorId ownerId, out bool added, Func<ActorId, T>? factory = null)
    {
        AddResult result = UpsertEntry(ownerId.Value, out bool exists);
        ref T componentRef = ref result.Component;

        if (exists)
        {
            added = false;
            return ref componentRef;
        }

        // Инициализируем только что созданный слот
        componentRef = factory?.Invoke(ownerId) ?? new T();

        Added?.Invoke(ownerId);
        ComponentAdded?.Invoke(ownerId, ref componentRef);

        added = true;
        return ref componentRef;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> GetValues()
    {
        return _values.AsSpan(0, _count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has(ActorId ownerId)
    {
        return ContainsEntry(ownerId.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(ActorId ownerId)
    {
        return RemoveEntry(ownerId.Value, out _);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(ActorId ownerId, out T component)
    {
        return RemoveEntry(ownerId.Value, out component);
    }

    public bool TryAdd(ActorId ownerId, in T component)
    {
        AddResult result = TryAddEntry(ownerId.Value);

        if (!result.IsSuccess)
        {
            return false;
        }

        ref T componentRef = ref result.Component;
        componentRef = component;

        Added?.Invoke(ownerId);
        ComponentAdded?.Invoke(ownerId, ref componentRef);

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T TryGet(uint ownerId)
    {
        return ref GetEntryRef(ownerId);
    }

    public bool Update(ActorId ownerId, in T component)
    {
        ref T exists = ref GetEntryRef(ownerId.Value);
        if (Unsafe.IsNullRef(ref exists))
        {
            return false;
        }

        ComponentUpdating?.Invoke(ownerId, ref exists, in component);

        exists = component;
        return true;
    }

    #region Interface

    ActorContext IActorComponentPool.Context
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Context;
    }

    void IActorComponentPool.Clone(ActorId ownerId, ActorId cloneId)
    {
        Clone(ownerId, cloneId);
    }

    IActorComponent IActorComponentPool.Get(ActorId ownerId)
    {
        return Get(ownerId);
    }

    #endregion
}
