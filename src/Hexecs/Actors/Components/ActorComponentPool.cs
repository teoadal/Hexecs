using Hexecs.Actors.Delegates;
using Hexecs.Actors.Serializations;
using Hexecs.Utils;

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

        _buckets = new int[capacity];
        _entries = new Entry[capacity];
        _freeCount = 0;
        _freeList = 0;
        _length = 0;

        _cloneHandler = configuration.CloneHandler;
        _converter = configuration.Converter;
        _disposeHandler = configuration.DisposeHandler;
    }

    public ref T Add(uint ownerId, in T component)
    {
        var entry = AddEntry(ownerId);

        ref var componentRef = ref entry.Entry.Value;
        componentRef = component;

        Added?.Invoke(ownerId);
        ComponentAdded?.Invoke(ownerId, entry.Index, in component);

        return ref componentRef;
    }

    public ref T Add(uint ownerId, T component)
    {
        var entry = AddEntry(ownerId);

        ref var componentRef = ref entry.Entry.Value;
        componentRef = component;

        Added?.Invoke(ownerId);
        ComponentAdded?.Invoke(ownerId, entry.Index, in component);

        return ref componentRef;
    }

    public void Clear()
    {
        ArrayUtils.Clear(_buckets);

        if (_disposeHandler != null)
        {
            var index = 0;
            while ((uint)index < (uint)_length)
            {
                ref var entry = ref _entries[index];
                if (entry.Next >= -1)
                {
                    _disposeHandler(ref entry.Value);
                }

                index++;
            }
        }

        ArrayUtils.Clear(_entries, _length);

        _freeCount = 0;
        _freeList = 0;
        _length = 0;
    }

    public ref T Clone(uint ownerId, uint cloneId)
    {
        ref var ownerEntry = ref GetEntry(ownerId);
        if (Unsafe.IsNullRef(ref ownerEntry)) ActorError.ComponentNotFound<T>(ownerId);

        if (_cloneHandler == null)
        {
            return ref Add(cloneId, in ownerEntry.Value);
        }

        var clone = _cloneHandler(in ownerEntry.Value);
        return ref Add(cloneId, clone);
    }

    public ActorRef<T> First()
    {
        var index = 0;
        while ((uint)index < (uint)_length)
        {
            ref var entry = ref _entries[index];
            if (entry.Next >= -1)
            {
                return new ActorRef<T>(Context, entry.Key, ref entry.Value);
            }

            index++;
        }

        return ActorRef<T>.Empty;
    }

    public ActorRef<T> First(ActorPredicate<T> predicate)
    {
        var index = 0;
        while ((uint)index < (uint)_length)
        {
            ref var entry = ref _entries[index];
            if (entry.Next >= -1)
            {
                var actor = new ActorRef<T>(Context, entry.Key, ref entry.Value);
                if (predicate(in actor))
                {
                    return actor;
                }
            }

            index++;
        }

        return ActorRef<T>.Empty;
    }

    public ref T Get(uint ownerId)
    {
        ref var entry = ref GetEntry(ownerId);
        if (Unsafe.IsNullRef(ref entry)) ActorError.ComponentNotFound<T>(ownerId);
        return ref entry.Value;
    }

    public ref T GetOrCreate(uint ownerId, out bool added, Func<uint, T>? factory = null)
    {
        var entry = AddEntry(ownerId, false);
        ref var componentRef = ref entry.Entry.Value;

        if (entry.Exists)
        {
            added = false;
            return ref componentRef;
        }

        var component = factory?.Invoke(ownerId) ?? new T();
        componentRef = component;

        Added?.Invoke(ownerId);
        ComponentAdded?.Invoke(ownerId, entry.Index, in component);

        added = true;
        return ref componentRef;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T GetByIndex(int index) => ref _entries[index].Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has(uint ownerId)
    {
        ref var entry = ref GetEntry(ownerId);
        return !Unsafe.IsNullRef(ref entry) && entry.Key == ownerId;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(uint ownerId) => Remove(ownerId, out _);

    public bool Remove(uint ownerId, out T component)
    {
        if (_length > 0)
        {
            ref var bucket = ref GetBucket(ownerId);
            var entries = _entries;
            var i = bucket - 1;
            var last = -1;

            while (i >= 0)
            {
                ref var entry = ref entries[i];
                if (entry.Key == ownerId)
                {
                    component = entry.Value; // copy

                    Removing?.Invoke(ownerId);
                    ComponentRemoving?.Invoke(ownerId, in component);

                    _disposeHandler?.Invoke(ref entry.Value);

                    if (last < 0) bucket = entry.Next + 1;
                    else entries[last].Next = entry.Next;

                    entry.Next = CollectionUtils.StartOfFreeList - _freeList;

                    _freeCount++;
                    _freeList = i;
                    return true;
                }

                last = i;
                i = entry.Next;
            }
        }

        component = default;
        return false;
    }

    public bool TryAdd(uint ownerId, in T component)
    {
        var entry = AddEntry(ownerId, false);

        if (entry.Exists) return false;

        ref var componentRef = ref entry.Entry.Value;
        componentRef = component;

        Added?.Invoke(ownerId);
        ComponentAdded?.Invoke(ownerId, entry.Index, in component);

        return true;
    }

    public ref T TryGet(uint ownerId)
    {
        ref var entry = ref GetEntry(ownerId);
        if (Unsafe.IsNullRef(ref entry)) return ref Unsafe.NullRef<T>();
        return ref entry.Value;
    }

    public int TryGetIndex(uint ownerId)
    {
        if (_length > 0)
        {
            var i = _buckets[ownerId % (uint)_buckets.Length] - 1;
            var entries = _entries;
            while ((uint)i < (uint)entries.Length)
            {
                ref var entry = ref entries[i];
                if (entry.Key == ownerId) return i;
                i = entry.Next;
            }
        }

        return -1;
    }

    public bool Update(uint ownerId, in T component)
    {
        ref var exists = ref TryGet(ownerId);
        if (Unsafe.IsNullRef(ref exists)) return false;

        ComponentUpdating?.Invoke(ownerId, in exists, component);

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