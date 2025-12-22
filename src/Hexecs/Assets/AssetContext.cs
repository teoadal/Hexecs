using Hexecs.Assets.Components;
using Hexecs.Worlds;

namespace Hexecs.Assets;

/// <summary>
/// Контекст ассетов, управляющий их жизненным циклом и содержащий коллекции их компонентов.
/// </summary>
[DebuggerDisplay("Length = {Length}")]
public sealed partial class AssetContext : IEnumerable<Asset>
{
    public readonly World World;

    private readonly Dictionary<string, uint> _aliases;

    internal AssetContext(World world, int capacity = 256)
    {
        World = world;

        _aliases = new Dictionary<string, uint>(ReferenceComparer<string>.Instance);
        _entries = new Dictionary<uint, Entry>(HashHelper.GetPrime(capacity));

        _componentPools = new IAssetComponentPool?[16];

        _filters = new Dictionary<Type, IAssetFilter>(8, ReferenceComparer<Type>.Instance);
        _filtersWithConstraint = new List<IAssetFilter>(8);
    }

    /// <summary>
    /// Проверяет существование ассета с указанным идентификатором.
    /// </summary>
    /// <param name="assetId">Идентификатор ассета для проверки</param>
    /// <returns>Возвращаем true, если ассет существует; иначе false</returns>
    public bool ExistsAsset(uint assetId)
    {
        ref var entry = ref GetEntry(assetId);
        return !Unsafe.IsNullRef(ref entry);
    }

    /// <summary>
    /// Получает ассет по его идентификатору.
    /// </summary>
    /// <param name="assetId">Идентификатор ассета</param>
    /// <returns>Объект ассета</returns>
    /// <exception cref="Exception">Выбрасывается, если ассет не найден</exception>
    public Asset GetAsset(uint assetId)
    {
        if (!ExistsAsset(assetId)) AssetError.NotFound(assetId);
        return new Asset(this, assetId);
    }

    /// <summary>
    /// Получает ассет по его алиасу.
    /// </summary>
    /// <param name="alias">Строковый алиас ассета</param>
    /// <returns>Объект ассета</returns>
    /// <exception cref="Exception">Выбрасывается, если ассет, связанный с алиасом не найден</exception>
    public Asset GetAsset(string alias)
    {
        if (_aliases.TryGetValue(alias, out var assetId))
        {
            return GetAsset(assetId);
        }

        AssetError.NotFound(alias);
        return Asset.Empty;
    }

    /// <summary>
    /// Получить первый ассет, у которого есть этот компонент.
    /// </summary>
    /// <typeparam name="T1">Тип компонента</typeparam>
    /// <exception cref="Exception">Ассет с таким компонентом не существует</exception>
    /// <returns>Первый ассет, у которого есть этот компонент.</returns>
    public Asset<T1> GetAsset<T1>()
        where T1 : struct, IAssetComponent
    {
        var pool = GetComponentPool<T1>();
        if (pool != null)
        {
            var firstId = pool.FirstId();
            if (firstId != Asset.EmptyId)
            {
                return new Asset<T1>(this, firstId);
            }
        }

        AssetError.NotFound<T1>();
        return Asset<T1>.Empty;
    }

    /// <summary>
    /// Получает типизированный ассет, содержащий указанный компонент.
    /// </summary>
    /// <typeparam name="T1">Тип компонента ассета</typeparam>
    /// <param name="assetId">Идентификатор ассета</param>
    /// <returns>Типизированный объект ассета</returns>
    /// <exception cref="Exception">Выбрасывается, если ассет не найден или не содержит нужный компонент</exception>
    public Asset<T1> GetAsset<T1>(uint assetId)
        where T1 : struct, IAssetComponent
    {
        var pool = GetComponentPool<T1>();
        if (pool == null || !pool.Has(assetId)) AssetError.ComponentNotFound<T1>(assetId);

        return new Asset<T1>(this, assetId);
    }

    /// <summary>
    /// Получает типизированный ассет по его алиасу.
    /// </summary>
    /// <typeparam name="T1">Тип компонента ассета</typeparam>
    /// <param name="alias">Строковый алиас ассета</param>
    /// <returns>Типизированный объект ассета</returns>
    /// <exception cref="Exception">Выбрасывается, если ассет не найден или не содержит нужный компонент</exception>
    public Asset<T1> GetAsset<T1>(string alias)
        where T1 : struct, IAssetComponent
    {
        if (_aliases.TryGetValue(alias, out var assetId))
        {
            return GetAsset<T1>(assetId);
        }

        AssetError.NotFound(alias);
        return Asset<T1>.Empty;
    }

    /// <summary>
    /// Получает ссылку на компонент ассета с указанным идентификатором.
    /// </summary>
    /// <typeparam name="T1">Тип компонента ассета</typeparam>
    /// <param name="assetId">Идентификатор ассета</param>
    /// <returns>Ссылка на компонент ассета</returns>
    /// <exception cref="Exception">Выбрасывается, если ассет не найден или не содержит нужный компонент</exception>
    public AssetRef<T1> GetAssetRef<T1>(uint assetId)
        where T1 : struct, IAssetComponent
    {
        Debug.Assert(ExistsAsset(assetId), $"Asset {assetId} isn't found");

        var pool = GetComponentPool<T1>();
        if (pool == null) AssetError.ComponentNotFound<T1>(assetId);

        ref var component = ref pool.Get(assetId);
        if (Unsafe.IsNullRef(ref component)) AssetError.ComponentNotFound<T1>(assetId);

        return new AssetRef<T1>(this, assetId, ref component);
    }

    /// <summary>
    /// Формирует строковое описание ассета с перечислением его компонентов.
    /// </summary>
    /// <param name="assetId">Идентификатор ассета</param>
    /// <param name="maxComponentDescription">Максимальное количество компонентов для отображения</param>
    /// <returns>Строковое описание ассета</returns>
    [SkipLocalsInit]
    public string GetDescription(uint assetId, int maxComponentDescription = 5)
    {
        var builder = new ValueStringBuilder(stackalloc char[512]);
        GetDescription(assetId, ref builder, maxComponentDescription);
        return builder.Flush();
    }

    public void GetDescription(uint assetId, ref ValueStringBuilder builder, int maxComponentDescription = 5)
    {
        ref var entry = ref GetEntry(assetId);
        if (Unsafe.IsNullRef(ref entry))
        {
            builder.Append('\'');
            builder.Append(StringUtils.EmptyValue);
            builder.Append('\'');
        }

        builder.Append("Id = ");
        builder.Append(assetId);

        var components = entry.AsReadOnlySpan();
        var componentsLength = components.Length;
        if (componentsLength == 0) return;

        builder.Append(" (");

        var pool = ArrayPool<string>.Shared;
        var buffer = pool.Rent(componentsLength);
        var index = 0;
        var printMore = false;

        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var componentId in components)
        {
            if (maxComponentDescription == index)
            {
                printMore = true;
                break;
            }

            ArrayUtils.Insert(
                ref buffer,
                pool,
                index++,
                TypeOf.GetTypeName(AssetComponentType.GetType(componentId))
            );
        }

        Array.Sort(buffer, 0, componentsLength);

        var first = true;
        foreach (var componentName in buffer.AsSpan(0, index))
        {
            if (first == false) builder.Append(", ");
            else first = false;

            builder.Append(componentName);
        }

        if (printMore) builder.Append(", ...");
        builder.Append(')');
    }

    /// <summary>
    /// Пытается получить типизированный ассет с указанным идентификатором.
    /// </summary>
    /// <typeparam name="T1">Тип компонента ассета</typeparam>
    /// <param name="assetId">Идентификатор ассета</param>
    /// <param name="asset">Результирующий типизированный ассет, если найден</param>
    /// <returns>Возвращает true, если ассет найден и содержит указанный компонент; иначе false</returns>
    public bool TryGetAsset<T1>(uint assetId, out Asset<T1> asset) where T1 : struct, IAssetComponent
    {
        var pool = GetComponentPool<T1>();
        if (pool == null || !pool.Has(assetId))
        {
            asset = Asset<T1>.Empty;
            return false;
        }

        asset = new Asset<T1>(this, assetId);
        return true;
    }

    /// <summary>
    /// Пытается получить ссылку на компонент ассета с указанным идентификатором.
    /// </summary>
    /// <typeparam name="T1">Тип компонента ассета</typeparam>
    /// <param name="assetId">Идентификатор ассета</param>
    /// <param name="asset">Результирующая ссылка на компонент ассета, если компонент найден</param>
    /// <returns>Возвращает true, если ассет найден и содержит указанный компонент; иначе false</returns>
    public bool TryGetAssetRef<T1>(uint assetId, out AssetRef<T1> asset) where T1 : struct, IAssetComponent
    {
        var pool = GetComponentPool<T1>();
        if (pool == null)
        {
            asset = AssetRef<T1>.Empty;
            return false;
        }

        ref var component = ref pool.TryGet(assetId);
        if (Unsafe.IsNullRef(ref component))
        {
            asset = AssetRef<T1>.Empty;
            return false;
        }

        asset = new AssetRef<T1>(this, assetId, ref component);
        return true;
    }
}