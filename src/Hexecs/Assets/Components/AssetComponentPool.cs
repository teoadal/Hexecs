using Hexecs.Assets.Delegates;

namespace Hexecs.Assets.Components;

[DebuggerDisplay("{Length}")]
internal sealed class AssetComponentPool<T> : IAssetComponentPool
    where T : struct, IAssetComponent
{
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
    public AssetRef<T> First()
    {
        return _count > 0
            ? new AssetRef<T>(Context, new AssetId(_dense[0]), ref _values[0])
            : AssetRef<T>.Empty;
    }

    public AssetRef<T> First(AssetPredicate<T> predicate)
    {
        int count = _count;
        uint[] keys = _dense;
        T[] values = _values;
        AssetContext context = Context;

        for (var i = 0; i < count; i++)
        {
            var actor = new AssetRef<T>(context, new AssetId(keys[i]), ref values[i]);
            if (predicate(in actor))
            {
                return actor;
            }
        }

        return AssetRef<T>.Empty;
    }
    
    public ref T Get(AssetId assetId)
    {
        uint assetIdRaw = assetId.Value;
        var pageIndex = (int)(assetIdRaw >> PageBits);
        if ((uint)pageIndex < (uint)_sparsePages.Length)
        {
            uint[]? page = _sparsePages[pageIndex];
            if (page != null)
            {
                uint denseIndexPlusOne = page[assetIdRaw & PageMask];
                if (denseIndexPlusOne != 0)
                {
                    int index = (int)denseIndexPlusOne - 1;
                    if (_dense[index] == assetIdRaw)
                    {
                        return ref _values[index];
                    }
                }
            }
        }

        return ref Unsafe.NullRef<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T GetByIndex(int index)
    {
        return ref _values[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has(AssetId assetId)
    {
        uint assetIdRaw = assetId.Value;
        var pageIndex = (int)(assetIdRaw >> PageBits);
        uint[]?[] pages = _sparsePages;

        if ((uint)pageIndex < (uint)pages.Length)
        {
            uint[]? page = pages[pageIndex];
            if (page != null)
            {
                uint denseIndexPlusOne = page[assetIdRaw & PageMask];
                return denseIndexPlusOne != 0 && _dense[denseIndexPlusOne - 1] == assetIdRaw;
            }
        }

        return false;
    }

    public ref T Set(AssetId assetId, in T component)
    {
        uint assetIdRaw = assetId.Value;
        var pageIndex = (int)(assetIdRaw >> PageBits);
        uint[]?[] pages = _sparsePages;

        // Максимально компактная проверка на готовность страницы и места
        if ((uint)pageIndex < (uint)pages.Length)
        {
            uint[]? page = pages[pageIndex];
            if (page != null && (uint)_count < (uint)_dense.Length)
            {
                ref uint slot = ref page[assetIdRaw & PageMask];
                if (slot == 0) // Чистая вставка (самый частый случай в ECS)
                {
                    var idx = (uint)_count;
                    slot = idx + 1;
                    _dense[idx] = assetIdRaw;
                    ref T internalRef = ref _values[idx];

                    _values[idx] = component;
                    _count++;

                    return ref internalRef;
                }

                // Если не 0, проверяем на дубликат (чуть медленнее)
                if (_dense[slot - 1] == assetIdRaw)
                {
                    AssetError.ComponentAlreadyExists<T>(assetId);
                }
            }
        }

        return ref SetSlow(assetId, in component);
    }

    public ref T TryGet(AssetId assetId)
    {
        uint assetIdRaw = assetId.Value;
        var pageIndex = (int)(assetIdRaw >> PageBits);
        if ((uint)pageIndex < (uint)_sparsePages.Length)
        {
            uint[]? page = _sparsePages[pageIndex];
            if (page != null)
            {
                uint denseIndexPlusOne = page[assetIdRaw & PageMask];
                if (denseIndexPlusOne != 0)
                {
                    int index = (int)denseIndexPlusOne - 1;
                    if (_dense[index] == assetIdRaw)
                    {
                        return ref _values[index];
                    }
                }
            }
        }

        return ref Unsafe.NullRef<T>();
    }

    public int TryGetIndex(AssetId assetId)
    {
        uint assetIdRaw = assetId.Value;

        var pageIndex = (int)(assetIdRaw >> PageBits);
        uint[]?[] pages = _sparsePages;

        if ((uint)pageIndex < (uint)pages.Length)
        {
            uint[]? page = pages[pageIndex];
            if (page != null)
            {
                uint slot = page[assetIdRaw & PageMask];
                if (slot != 0)
                {
                    int denseIndex = (int)slot - 1;
                    if (_dense[denseIndex] == assetIdRaw)
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
            int newSize = _dense.Length * 2;
            Array.Resize(ref _dense, newSize);
            Array.Resize(ref _values, newSize);
        }
    }

    private void EnsurePageArraySize(int pageIndex)
    {
        if (pageIndex >= _sparsePages.Length)
        {
            int newSize = Math.Max(_sparsePages.Length * 2, pageIndex + 1);
            Array.Resize(ref _sparsePages, newSize);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private ref T SetSlow(AssetId assetId, in T component)
    {
        uint assetIdRaw = assetId.Value;

        EnsureDenseCapacity();
        var pageIndex = (int)(assetIdRaw >> PageBits);
        EnsurePageArraySize(pageIndex);

        ref uint[]? page = ref _sparsePages[pageIndex];
        if (page == null)
        {
            page = ArrayUtils.Create<uint>(PageSize);
            Array.Clear(page, 0, page.Length);
        }

        ref uint denseIndexPlusOne = ref page[assetIdRaw & PageMask];
        if (denseIndexPlusOne != 0)
        {
            if (_dense[denseIndexPlusOne - 1] == assetIdRaw)
            {
                AssetError.ComponentAlreadyExists<T>(assetId);
            }
        }

        var denseIndex = (uint)_count;
        denseIndexPlusOne = denseIndex + 1;
        _dense[denseIndex] = assetIdRaw;

        ref T internalRef = ref _values[denseIndex];
        internalRef = component;

        _count++;

        return ref internalRef;
    }

    AssetContext IAssetComponentPool.Context
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Context;
    }

    IAssetComponent IAssetComponentPool.Get(AssetId assetId)
    {
        return Get(assetId);
    }
}