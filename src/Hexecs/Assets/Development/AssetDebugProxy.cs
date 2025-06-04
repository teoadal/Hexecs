namespace Hexecs.Assets.Development;

[ExcludeFromCodeCoverage]
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
internal sealed class AssetDebugProxy
{
    public IAssetComponent[] Components => _components ??= _asset.IsEmpty
        ? []
        : _asset.Context.Components(_asset.Id).ToArray();

    private IAssetComponent[]? _components;
    private readonly Asset _asset;

    public AssetDebugProxy(Asset asset)
    {
        _asset = asset;
    }
}

[ExcludeFromCodeCoverage]
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
internal sealed class AssetDebugProxy<T1>(Asset<T1> asset)
    where T1 : struct, IAssetComponent
{
    public T1 Component1
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => asset.Context.GetComponent<T1>(asset.Id);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public IAssetComponent[] Components => _components ??= asset.IsEmpty
        ? []
        : asset.Context.Components(asset.Id).ToArray();

    private IAssetComponent[]? _components;
}