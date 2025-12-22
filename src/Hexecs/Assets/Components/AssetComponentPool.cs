namespace Hexecs.Assets.Components;

[DebuggerDisplay("{Length}")]
internal sealed class AssetComponentPool<T> : IAssetComponentPool
    where T : struct, IAssetComponent
{
    //private const int EmptySlot = 0;

    private const int PageBits = 12;
    private const int PageSize = 1 << PageBits; // 4096
    private const int PageMask = PageSize - 1;

    public readonly AssetContext Context;

    public ushort Id
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => AssetComponentType<T>.Id;
    }

    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _count;
    }

    private uint[]?[] _sparsePages;
    private uint[] _dense;
    private T[] _values;
    private int _count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AssetComponentPool(AssetContext context, int capacity = 8)
    {
        Context = context;

        _sparsePages = new uint[1][];
        _dense = new uint[capacity];
        _values = new T[capacity];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint FirstId() => _count > 0
        ? _dense[0]
        : Asset.EmptyId;

    public ref T Get(uint assetId)
    {
        var pageIndex = (int)(assetId >> PageBits);
        if ((uint)pageIndex < (uint)_sparsePages.Length)
        {
            var page = _sparsePages[pageIndex];
            if (page != null)
            {
                var denseIndexPlusOne = page[assetId & PageMask];
                if (denseIndexPlusOne != 0)
                {
                    var index = (int)denseIndexPlusOne - 1;
                    if (_dense[index] == assetId)
                    {
                        return ref _values[index];
                    }
                }
            }
        }

        return ref Unsafe.NullRef<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T GetByIndex(int index) => ref _values[index];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has(uint assetId)
    {
        var pageIndex = (int)(assetId >> PageBits);
        var pages = _sparsePages;

        if ((uint)pageIndex < (uint)pages.Length)
        {
            var page = pages[pageIndex];
            if (page != null)
            {
                var denseIndexPlusOne = page[assetId & PageMask];
                return denseIndexPlusOne != 0 && _dense[denseIndexPlusOne - 1] == assetId;
            }
        }

        return false;
    }

    public ref T Set(uint assetId, in T component)
    {
        var pageIndex = (int)(assetId >> PageBits);
        var pages = _sparsePages;

        // Максимально компактная проверка на готовность страницы и места
        if ((uint)pageIndex < (uint)pages.Length)
        {
            var page = pages[pageIndex];
            if (page != null && (uint)_count < (uint)_dense.Length)
            {
                ref var slot = ref page[assetId & PageMask];
                if (slot == 0) // Чистая вставка (самый частый случай в ECS)
                {
                    var idx = (uint)_count;
                    slot = idx + 1;
                    _dense[idx] = assetId;
                    ref var internalRef = ref _values[idx];

                    _values[idx] = component;
                    _count++;

                    return ref internalRef;
                }

                // Если не 0, проверяем на дубликат (чуть медленнее)
                if (_dense[slot - 1] == assetId)
                {
                    AssetError.ComponentAlreadyExists<T>(assetId);
                }
            }
        }

        return ref SetSlow(assetId, in component);
    }

    public ref T TryGet(uint assetId)
    {
        var pageIndex = (int)(assetId >> PageBits);
        if ((uint)pageIndex < (uint)_sparsePages.Length)
        {
            var page = _sparsePages[pageIndex];
            if (page != null)
            {
                var denseIndexPlusOne = page[assetId & PageMask];
                if (denseIndexPlusOne != 0)
                {
                    var index = (int)denseIndexPlusOne - 1;
                    if (_dense[index] == assetId)
                    {
                        return ref _values[index];
                    }
                }
            }
        }

        return ref Unsafe.NullRef<T>();
    }

    public int TryGetIndex(uint assetId)
    {
        var pageIndex = (int)(assetId >> PageBits);
        var pages = _sparsePages;

        if ((uint)pageIndex < (uint)pages.Length)
        {
            var page = pages[pageIndex];
            if (page != null)
            {
                var slot = page[assetId & PageMask];
                if (slot != 0)
                {
                    var denseIndex = (int)slot - 1;
                    if (_dense[denseIndex] == assetId)
                    {
                        return denseIndex;
                    }
                }
            }
        }

        return -1;
    }

    private void EnsureDenseCapacity()
    {
        if (_count >= _dense.Length)
        {
            var newSize = _dense.Length * 2;
            Array.Resize(ref _dense, newSize);
            Array.Resize(ref _values, newSize);
        }
    }

    private void EnsurePageArraySize(int pageIndex)
    {
        if (pageIndex >= _sparsePages.Length)
        {
            var newSize = Math.Max(_sparsePages.Length * 2, pageIndex + 1);
            Array.Resize(ref _sparsePages, newSize);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private ref T SetSlow(uint assetId, in T component)
    {
        EnsureDenseCapacity();
        var pageIndex = (int)(assetId >> PageBits);
        EnsurePageArraySize(pageIndex);

        ref var page = ref _sparsePages[pageIndex];
        if (page == null)
        {
            page = ArrayUtils.Create<uint>(PageSize);
            Array.Clear(page, 0, page.Length);
        }

        ref var denseIndexPlusOne = ref page[assetId & PageMask];
        if (denseIndexPlusOne != 0)
        {
            if (_dense[denseIndexPlusOne - 1] == assetId)
            {
                AssetError.ComponentAlreadyExists<T>(assetId);
            }
        }

        var denseIndex = (uint)_count;
        denseIndexPlusOne = denseIndex + 1;
        _dense[denseIndex] = assetId;

        ref var internalRef = ref _values[denseIndex];
        internalRef = component;

        _count++;

        return ref internalRef;
    }

    AssetContext IAssetComponentPool.Context
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Context;
    }

    IAssetComponent IAssetComponentPool.Get(uint assetId) => Get(assetId);
}