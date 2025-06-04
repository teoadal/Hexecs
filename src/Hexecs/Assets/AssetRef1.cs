using Hexecs.Assets.Development;

namespace Hexecs.Assets;

/// <summary>
/// Ссылка на ассет с компонентом.
/// </summary>
/// <remarks>
/// Используется аналогично структуре <see cref="Asset"/>, но для работы с отдельными компонентами.
/// </remarks>
/// <typeparam name="T1">Тип компонента ассета, должен быть структурой и реализовывать интерфейс <see cref="IAssetComponent"/>.</typeparam>
[DebuggerDisplay("{ToString()}")]
[DebuggerTypeProxy(typeof(AssetRefDebugProxy<>))]
public readonly ref struct AssetRef<T1>
    where T1 : struct, IAssetComponent
{
    /// <summary>
    /// Возвращает пустой экземпляр ссылки на ассет с компонентом.
    /// </summary>
    public static AssetRef<T1> Empty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(null!, Asset.EmptyId, ref Unsafe.NullRef<T1>());
    }

    /// <summary>
    /// Первый компонент ассета.
    /// </summary>
    public ref readonly T1 Component1
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _component1;
    }

    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Context == null;
    }

    public readonly AssetContext Context;
    public readonly uint Id;

    private readonly ref T1 _component1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal AssetRef(AssetContext context, uint id, ref T1 component1)
    {
        Context = context;
        Id = id;

        _component1 = ref component1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Asset<T> As<T>() where T : struct, IAssetComponent => Context.GetAsset<T>(Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AssetRef<T> AsRef<T>() where T : struct, IAssetComponent => Context.GetAssetRef<T>(Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref readonly T Get<T>() where T : struct, IAssetComponent => ref Context.GetComponent<T>(Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has<T>() where T : struct, IAssetComponent => Context.HasComponent<T>(Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Is<T>(out Asset<T> asset) where T : struct, IAssetComponent => Context.TryGetAsset(Id, out asset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsRef<T>(out AssetRef<T> asset) where T : struct, IAssetComponent
    {
        return Context.TryGetAssetRef(Id, out asset);
    }

    public override string ToString() => Context == null
        ? StringUtils.EmptyValue
        : Context.GetDescription(Id);

    #region Equality

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(AssetRef<T1> other) => Id == other.Id && ReferenceEquals(Context, other.Context);

    public override bool Equals(object? obj) => obj switch
    {
        Asset<T1> other => other.Id == Id,
        Asset asset => asset.IsRef<T1>(out var expected) && Equals(expected),
        _ => false
    };

    public override int GetHashCode() => HashCode.Combine(Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in AssetRef<T1> left, in AssetRef<T1> right) => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in AssetRef<T1> left, in AssetRef<T1> right) => !left.Equals(right);

    #endregion

    #region Implicit

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator bool(in AssetRef<T1> asset) => !asset.IsEmpty;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator AssetId(in AssetRef<T1> asset) => new(asset.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator AssetId<T1>(in AssetRef<T1> asset) => new(asset.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Asset(in AssetRef<T1> asset) => new(asset.Context, asset.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Asset<T1>(in AssetRef<T1> asset) => new(asset.Context, asset.Id);

    #endregion
}