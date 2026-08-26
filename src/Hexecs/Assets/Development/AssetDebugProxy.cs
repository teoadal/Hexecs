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