using Hexecs.Assets.Components;

namespace Hexecs.Assets;

public sealed partial class AssetContext
{
    private IAssetComponentPool?[] _componentPools;
#if NET9_0_OR_GREATER
    private readonly Lock _componentPoolLock = new();
#else
    private readonly object _componentPoolLock = new();
#endif

    /// <summary>
    /// Возвращает перечислитель компонентов для указанного ассета.
    /// </summary>
    /// <param name="assetId">Идентификатор ассета</param>
    /// <returns>Перечислитель компонентов ассета</returns>
    public ComponentEnumerator Components(uint assetId)
    {
        ref var entry = ref GetEntry(assetId);
        return Unsafe.IsNullRef(ref entry)
            ? ComponentEnumerator.Empty
            : new ComponentEnumerator(assetId, _componentPools, entry.AsReadOnlySpan());
    }

    /// <summary>
    /// Проверяет наличие компонента указанного типа в ассете.
    /// </summary>
    /// <typeparam name="T">Тип компонента</typeparam>
    /// <param name="assetId">Идентификатор ассета</param>
    /// <returns>Возвращает true, если компонент существует; иначе false</returns>
    public bool HasComponent<T>(uint assetId) where T : struct, IAssetComponent
    {
        var pool = GetComponentPool<T>();
        return pool != null && pool.Has(assetId);
    }

    /// <summary>
    /// Возвращает компонент указанного типа для ассета.
    /// </summary>
    /// <typeparam name="T">Тип компонента</typeparam>
    /// <param name="assetId">Идентификатор ассета</param>
    /// <returns>Ссылка на компонент ассета</returns>
    /// <exception cref="Exception">Возникает, если компонент не найден</exception>
    public ref readonly T GetComponent<T>(uint assetId) where T : struct, IAssetComponent
    {
        var pool = GetComponentPool<T>();
        if (pool == null) AssetError.ComponentNotFound<T>(assetId);
        return ref pool.Get(assetId);
    }

    public AssetComponentRef<T> GetComponentRef<T>(uint actorId) where T : struct, IAssetComponent
    {
        var pool = GetComponentPool<T>();
        if (pool == null) return AssetComponentRef<T>.Empty;

        var index = pool.TryGetIndex(actorId);

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
    internal AssetComponentPool<T>? GetComponentPool<T>() where T : struct, IAssetComponent
    {
        var id = AssetComponentType<T>.Id;

        if (id >= _componentPools.Length) return null;
        var pool = _componentPools[id];
        return pool == null
            ? null
            : Unsafe.As<AssetComponentPool<T>>(pool);
    }

    /// <summary>
    /// Возвращает существующий или создает новый пул компонентов указанного типа.
    /// </summary>
    /// <typeparam name="T">Тип компонента</typeparam>
    /// <returns>Пул компонентов указанного типа</returns>
    internal AssetComponentPool<T> GetOrCreateComponentPool<T>() where T : struct, IAssetComponent
    {
        var id = AssetComponentType<T>.Id;
        if (id < _componentPools.Length)
        {
            var existsPool = _componentPools[id];
            if (existsPool != null) return Unsafe.As<AssetComponentPool<T>>(existsPool);
        }
#if NET9_0_OR_GREATER
        using (_componentPoolLock.EnterScope())
#else
        lock (_componentPoolLock)
#endif
        {
            ArrayUtils.EnsureCapacity(ref _componentPools, id);
            ref var pool = ref _componentPools[id];
            pool ??= new AssetComponentPool<T>(this);

            return Unsafe.As<AssetComponentPool<T>>(pool);
        }
    }
}