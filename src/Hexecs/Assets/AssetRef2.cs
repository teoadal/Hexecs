using Hexecs.Assets.Development;

namespace Hexecs.Assets;

/// <summary>
/// Ссылка на ассет с двумя компонентами.
/// </summary>
/// <remarks>
/// Используется аналогично структуре <see cref="Asset"/>, но для работы с отдельными компонентами.
/// </remarks>
/// <typeparam name="T1">Тип компонента ассета, должен быть структурой и реализовывать интерфейс <see cref="IAssetComponent"/>.</typeparam>
/// <typeparam name="T2">Тип компонента ассета, должен быть структурой и реализовывать интерфейс <see cref="IAssetComponent"/>.</typeparam>
[DebuggerDisplay("{ToString()}")]
[DebuggerTypeProxy(typeof(AssetRefDebugProxy<,>))]
public readonly ref struct AssetRef<T1, T2>
    where T1 : struct, IAssetComponent
    where T2 : struct, IAssetComponent
{
    /// <summary>
    /// Возвращает пустой экземпляр ссылки на ассет с двумя компонентами.
    /// </summary>
    public static AssetRef<T1, T2> Empty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(null!, Asset.EmptyId, ref Unsafe.NullRef<T1>(), ref Unsafe.NullRef<T2>());
    }

    /// <summary>
    /// Первый компонент ассета.
    /// </summary>
    public ref readonly T1 Component1
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _component1;
    }

    /// <summary>
    /// Второй компонент ассета.
    /// </summary>
    public ref readonly T2 Component2
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _component2;
    }

    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Context == null;
    }

    public readonly AssetContext Context;
    public readonly uint Id;

    private readonly ref T1 _component1;
    private readonly ref T2 _component2;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal AssetRef(AssetContext context, uint id, ref T1 component1, ref T2 component2)
    {
        Context = context;
        Id = id;

        _component1 = ref component1;
        _component2 = ref component2;
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
    public bool Equals(AssetRef<T1, T2> other) => Id == other.Id && ReferenceEquals(Context, other.Context);

    public override bool Equals(object? obj) => obj is Asset other && Id == other.Id;

    public override int GetHashCode() => HashCode.Combine(Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in AssetRef<T1, T2> left, in AssetRef<T1, T2> right) => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in AssetRef<T1, T2> left, in AssetRef<T1, T2> right) => !left.Equals(right);

    #endregion

    #region Implicit

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator bool(in AssetRef<T1, T2> asset) => !asset.IsEmpty;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator AssetId(in AssetRef<T1, T2> asset) => new(asset.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator AssetId<T1>(in AssetRef<T1, T2> asset) => new(asset.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator AssetId<T2>(in AssetRef<T1, T2> asset) => new(asset.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Asset(in AssetRef<T1, T2> asset) => new(asset.Context, asset.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Asset<T1>(in AssetRef<T1, T2> asset) => new(asset.Context, asset.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Asset<T2>(in AssetRef<T1, T2> asset) => new(asset.Context, asset.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator AssetRef<T1>(in AssetRef<T1, T2> actor) => new(
        actor.Context,
        actor.Id,
        ref actor._component1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator AssetRef<T2>(in AssetRef<T1, T2> actor) => new(
        actor.Context,
        actor.Id,
        ref actor._component2);

    #endregion
}