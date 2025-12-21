using Hexecs.Actors.Delegates;
using Hexecs.Actors.Serializations;

namespace Hexecs.Actors.Components;

[SuppressMessage("ReSharper", "InvertIf")]
[SuppressMessage("ReSharper", "StaticMemberInGenericType")]
[DebuggerDisplay("{Length}")]
internal sealed partial class ActorComponentPool<T> : IActorComponentPool
    where T : struct, IActorComponent
{
    public event Action<uint>? Added;
    public event Action<uint>? Removing;

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

        var capacity = HashHelper.GetPrime(configuration.Capacity ?? 16);

        _sparsePages = new uint[16][];
        _dense = new uint[capacity];
        _values = new T[capacity];

        _cloneHandler = configuration.CloneHandler;
        _converter = configuration.Converter;
        _disposeHandler = configuration.DisposeHandler;
    }

    public ref T Add(uint ownerId, in T component)
    {
        var result = TryAddEntry(ownerId);
        if (result.IsSuccess)
        {
            ref var componentRef = ref result.Component;
            componentRef = component;

            Added?.Invoke(ownerId);
            ComponentAdded?.Invoke(ownerId, result.Index, ref componentRef);

            return ref componentRef;
        }

        ActorError.ComponentExists<T>(ownerId);
        return ref Unsafe.NullRef<T>();
    }

    public void Clear()
    {
        var dense = _dense;
        var sparsePages = _sparsePages;

        if (_disposeHandler != null)
        {
            foreach (ref var component in _values.AsSpan(0, _count))
            {
                _disposeHandler(ref component);
            }
        }

        for (var i = 0; i < _count; i++)
        {
            var key = dense[i];
            var pageIndex = (int)(key >> PageBits);
            sparsePages[pageIndex]![key & PageMask] = 0;
        }

        _count = 0;
    }

    public ref T Clone(uint ownerId, uint cloneId)
    {
        ref var ownerEntry = ref GetEntryRef(ownerId);
        if (Unsafe.IsNullRef(ref ownerEntry)) ActorError.ComponentNotFound<T>(ownerId);

        if (_cloneHandler == null)
        {
            return ref Add(cloneId, in ownerEntry);
        }

        var clone = _cloneHandler(in ownerEntry);
        return ref Add(cloneId, Unsafe.AsRef(in clone));
    }

    public ActorRef<T> First()
    {
        return _count > 0
            ? new ActorRef<T>(Context, _dense[0], ref _values[0])
            : ActorRef<T>.Empty;
    }

    public ActorRef<T> First(ActorPredicate<T> predicate)
    {
        var count = _count;
        var keys = _dense;
        var values = _values;
        var context = Context;

        for (var i = 0; i < count; i++)
        {
            var actor = new ActorRef<T>(context, keys[i], ref values[i]);
            if (predicate(in actor))
            {
                return actor;
            }
        }

        return ActorRef<T>.Empty;
    }

    public ref T Get(uint ownerId)
    {
        ref var entry = ref GetEntryRef(ownerId);
        if (Unsafe.IsNullRef(ref entry)) ActorError.ComponentNotFound<T>(ownerId);
        return ref entry;
    }

    public ref T GetOrCreate(uint ownerId, out bool added, Func<uint, T>? factory = null)
    {
        var result = UpsertEntry(ownerId, out var exists);
        ref var componentRef = ref result.Component;

        if (exists)
        {
            added = false;
            return ref componentRef;
        }

        // Инициализируем только что созданный слот
        componentRef = factory?.Invoke(ownerId) ?? new T();

        Added?.Invoke(ownerId);
        ComponentAdded?.Invoke(ownerId, result.Index, ref componentRef);

        added = true;
        return ref componentRef;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T GetByIndex(int index) => ref _values[index];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> GetValues() => _values.AsSpan(0, _count);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has(uint ownerId) => ContainsEntry(ownerId);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(uint ownerId) => RemoveEntry(ownerId, out _);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(uint ownerId, out T component) => RemoveEntry(ownerId, out component);

    public bool TryAdd(uint ownerId, in T component)
    {
        var result = TryAddEntry(ownerId);

        if (!result.IsSuccess) return false;

        ref var componentRef = ref result.Component;
        componentRef = component;

        Added?.Invoke(ownerId);
        ComponentAdded?.Invoke(ownerId, result.Index, ref componentRef);

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T TryGet(uint ownerId) => ref GetEntryRef(ownerId);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int TryGetIndex(uint ownerId) => TryGetEntryIndex(ownerId);

    public bool Update(uint ownerId, in T component)
    {
        ref var exists = ref GetEntryRef(ownerId);
        if (Unsafe.IsNullRef(ref exists)) return false;

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

    void IActorComponentPool.Clone(uint ownerId, uint cloneId) => Clone(ownerId, cloneId);

    IActorComponent IActorComponentPool.Get(uint ownerId) => Get(ownerId);

    #endregion
}