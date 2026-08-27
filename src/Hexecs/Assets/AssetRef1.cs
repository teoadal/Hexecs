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
public readonly ref struct AssetRef<T1> : IEquatable<Asset>
    where T1 : struct, IAssetComponent
{
    /// <summary>
    /// Возвращает пустой экземпляр ссылки на ассет с компонентом.
    /// </summary>
    public static AssetRef<T1> Empty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new AssetRef<T1>(null!, AssetId.Empty, ref Unsafe.NullRef<T1>());
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
    public readonly AssetId Id;

    private readonly ref T1 _component1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal AssetRef(AssetContext context, AssetId id, ref T1 component1)
    {
        Context = context;
        Id = id;

        _component1 = ref component1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AssetRef<T> AsRef<T>()
        where T : struct, IAssetComponent
    {
        return Context.GetAssetRef<T>(Id);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref readonly T Get<T>()
        where T : struct, IAssetComponent
    {
        return ref Context.GetComponent<T>(Id);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has<T>() where T : struct, IAssetComponent
    {
        return Context.HasComponent<T>(Id);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsRef<T>(out AssetRef<T> asset) where T : struct, IAssetComponent
    {
        return Context.TryGetAssetRef(Id, out asset);
    }

    public override string ToString()
    {
        return Context == null
            ? StringUtils.EmptyValue
            : Context.GetDescription(Id);
    }

    #region Equality

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Asset other)
    {
        return Id == other.Id && ReferenceEquals(Context, other.Context);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(AssetRef<T1> other)
    {
        return Id == other.Id && ReferenceEquals(Context, other.Context);
    }

    public override bool Equals(object? obj)
    {
        return obj switch
        {
            Asset asset => asset.IsRef<T1>(out AssetRef<T1> expected) && Equals(expected),
            _ => false
        };
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in AssetRef<T1> left, in AssetRef<T1> right)
    {
        return left.Equals(right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in AssetRef<T1> left, in AssetRef<T1> right)
    {
        return !left.Equals(right);
    }

    #endregion

    #region Implicit

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator bool(in AssetRef<T1> asset)
    {
        return !asset.IsEmpty;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator AssetId(in AssetRef<T1> asset)
    {
        return asset.Id;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Asset(in AssetRef<T1> asset)
    {
        return new Asset(asset.Context, asset.Id);
    }

    #endregion
}
