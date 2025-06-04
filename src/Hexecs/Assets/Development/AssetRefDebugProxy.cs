namespace Hexecs.Assets.Development;

[ExcludeFromCodeCoverage]
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
internal sealed class AssetRefDebugProxy<T1>(AssetRef<T1> asset)
    where T1 : struct, IAssetComponent
{
    public T1 Component1
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _context.GetComponent<T1>(_assetId);
    }

    public IAssetComponent[] Components => _components ??= _context.ExistsAsset(_assetId)
        ? _context.Components(_assetId).ToArray()
        : [];

    private readonly uint _assetId = asset.Id;
    private readonly AssetContext _context = asset.Context;
    private IAssetComponent[]? _components;
}

[ExcludeFromCodeCoverage]
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
internal sealed class AssetRefDebugProxy<T1, T2>(AssetRef<T1, T2> asset)
    where T1 : struct, IAssetComponent
    where T2 : struct, IAssetComponent
{
    public T1 Component1
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _context.GetComponent<T1>(_assetId);
    }

    public T2 Component2
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _context.GetComponent<T2>(_assetId);
    }

    public IAssetComponent[] Components => _components ??= _context.ExistsAsset(_assetId)
        ? _context.Components(_assetId).ToArray()
        : [];

    private readonly uint _assetId = asset.Id;
    private readonly AssetContext _context = asset.Context;
    private IAssetComponent[]? _components;
}

[ExcludeFromCodeCoverage]
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
internal sealed class AssetRefDebugProxy<T1, T2, T3>(AssetRef<T1, T2, T3> asset)
    where T1 : struct, IAssetComponent
    where T2 : struct, IAssetComponent
    where T3 : struct, IAssetComponent
{
    public T1 Component1
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _context.GetComponent<T1>(_assetId);
    }

    public T2 Component2
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _context.GetComponent<T2>(_assetId);
    }

    public T3 Component3
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _context.GetComponent<T3>(_assetId);
    }

    public IAssetComponent[] Components => _components ??= _context.ExistsAsset(_assetId)
        ? _context.Components(_assetId).ToArray()
        : [];

    private readonly uint _assetId = asset.Id;
    private readonly AssetContext _context = asset.Context;
    private IAssetComponent[]? _components;
}