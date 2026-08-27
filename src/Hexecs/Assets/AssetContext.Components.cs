using Hexecs.Assets.Components;

namespace Hexecs.Assets;

public sealed partial class AssetContext
{
    private IAssetComponentPool?[] _componentPools;
#if NET9_0_OR_GREATER
    private readonly Lock _componentPoolLock = new Lock();
#else
    private readonly object _componentPoolLock = new object();
#endif

    /// <summary>
    /// Возвращает перечислитель компонентов для указанного ассета.
    /// </summary>
    /// <param name="assetId">Идентификатор ассета</param>
    /// <returns>Перечислитель компонентов ассета</returns>
    public ComponentEnumerator Components(AssetId assetId)
    {
        ref Entry entry = ref GetEntry(assetId.Value);
        return Unsafe.IsNullRef(ref entry)
            ? ComponentEnumerator.Empty
            : new ComponentEnumerator(assetId, _componentPools, entry.ToArray());
    }

    /// <summary>
    /// Проверяет наличие компонента указанного типа в ассете.
    /// </summary>
    /// <typeparam name="T">Тип компонента</typeparam>
    /// <param name="assetId">Идентификатор ассета</param>
    /// <returns>Возвращает true, если компонент существует; иначе false</returns>
    public bool HasComponent<T>(AssetId assetId)
        where T : struct, IAssetComponent
    {
        AssetComponentPool<T>? pool = GetComponentPool<T>();
        return pool != null && pool.Has(assetId);
    }

    /// <summary>
    /// Возвращает компонент указанного типа для ассета.
    /// </summary>
    /// <typeparam name="T">Тип компонента</typeparam>
    /// <param name="assetId">Идентификатор ассета</param>
    /// <returns>Ссылка на компонент ассета</returns>
    /// <exception cref="Exception">Возникает, если компонент не найден</exception>
    public ref readonly T GetComponent<T>(AssetId assetId)
        where T : struct, IAssetComponent
    {
        AssetComponentPool<T>? pool = GetComponentPool<T>();
        if (pool == null)
        {
            AssetError.ComponentNotFound<T>(assetId);
        }

        return ref pool.Get(assetId);
    }

    public AssetComponentRef<T> GetComponentRef<T>(AssetId assetId)
        where T : struct, IAssetComponent
    {
        AssetComponentPool<T>? pool = GetComponentPool<T>();
        if (pool == null)
        {
            return AssetComponentRef<T>.Empty;
        }

        int index = pool.TryGetIndex(assetId);

        return index == -1
            ? AssetComponentRef<T>.Empty
            : new AssetComponentRef<T>(pool, index);
    }

    /// <summary>
    /// Возвращает пул компонентов указанного типа.
    /// </summary>
    /// <typeparam name="T">Тип компонента</typeparam>
    /// <returns>Пул компонентов указанного типа или null, если пул не существует</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal AssetComponentPool<T>? GetComponentPool<T>()
        where T : struct, IAssetComponent
    {
        ushort id = AssetComponentType<T>.Id;

        if (id >= _componentPools.Length)
        {
            return null;
        }

        IAssetComponentPool? pool = _componentPools[id];
        return pool == null
            ? null
            : Unsafe.As<AssetComponentPool<T>>(pool);
    }

    /// <summary>
    /// Возвращает существующий или создает новый пул компонентов указанного типа.
    /// </summary>
    /// <typeparam name="T">Тип компонента</typeparam>
    /// <returns>Пул компонентов указанного типа</returns>
    internal AssetComponentPool<T> GetOrCreateComponentPool<T>()
        where T : struct, IAssetComponent
    {
        ushort id = AssetComponentType<T>.Id;
        if (id < _componentPools.Length)
        {
            IAssetComponentPool? existsPool = _componentPools[id];
            if (existsPool != null)
            {
                return Unsafe.As<AssetComponentPool<T>>(existsPool);
            }
        }
#if NET9_0_OR_GREATER
        using (_componentPoolLock.EnterScope())
#else
        lock (_componentPoolLock)
#endif
        {
            ArrayUtils.EnsureCapacity(ref _componentPools, id);
            ref IAssetComponentPool? pool = ref _componentPools[id];
            pool ??= new AssetComponentPool<T>(this);

            return Unsafe.As<AssetComponentPool<T>>(pool);
        }
    }
}