namespace Hexecs.Assets.Components;

[DebuggerDisplay("{Length}")]
internal sealed class AssetComponentPool<T> : IAssetComponentPool
    where T : struct, IAssetComponent
{
    private const int EmptySlot = 0;

    public readonly AssetContext Context;

    public ushort Id
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => AssetComponentType<T>.Id;
    }

    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _length;
    }

    private T[] _components;
    private int[] _ids;
    private int _nextFreeSlot;
    private int _length;

    public AssetComponentPool(AssetContext context, int capacity = 8)
    {
        Context = context;

        capacity = HashHelper.GetPrime(capacity);
        _components =  ArrayUtils.Create<T>(capacity);
        _ids = new int[capacity];
    }

    public AssetRef<T> First()
    {
        for (uint i = 0; i < _ids.Length; i++)
        {
            var slot = _ids[i];
            if (slot == EmptySlot) continue;

            return new AssetRef<T>(Context, i, ref _components[slot]);
        }

        return AssetRef<T>.Empty;
    }

    public ref T Get(uint ownerId)
    {
        var slot = TryGetIndex(ownerId);
        if (slot != -1 && slot <= _components.Length)
        {
            return ref _components[slot];
        }

        AssetError.ComponentNotFound<T>(ownerId);
        return ref Unsafe.NullRef<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T GetByIndex(int index) => ref _components[index];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has(uint ownerId) => ownerId <= _ids.Length && _ids[ownerId] != EmptySlot;

    public ref T Set(uint ownerId, in T component)
    {
        var idValue = (int)ownerId;
        ArrayUtils.EnsureCapacity(ref _ids, idValue + 1);

        ref var id = ref _ids[idValue];
        if (id != EmptySlot) AssetError.ComponentAlreadyExists<T>(ownerId);

        var slot = Interlocked.Increment(ref _nextFreeSlot);
        ArrayUtils.EnsureCapacity(ref _components, _nextFreeSlot);

        id = slot;

        ref var reference = ref _components[slot];
        reference = component;

        Interlocked.Increment(ref _length);

        return ref reference;
    }

    public ref T TryGet(uint ownerId)
    {
        var slot = TryGetIndex(ownerId);
        if (slot != -1 && slot <= _components.Length)
        {
            return ref _components[slot];
        }

        return ref Unsafe.NullRef<T>();
    }

    public int TryGetIndex(uint ownerId)
    {
        // ReSharper disable once InvertIf
        if (ownerId < _ids.Length)
        {
            var slot = _ids[ownerId];
            if (slot != EmptySlot) return slot;
        }

        return -1;
    }

    AssetContext IAssetComponentPool.Context
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Context;
    }

    IAssetComponent IAssetComponentPool.Get(uint ownerId) => Get(ownerId);
}