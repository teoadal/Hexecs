namespace Hexecs.Assets.Development;

[ExcludeFromCodeCoverage]
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
internal sealed class AssetIdDebugProxy(AssetId assetId)
{
    public IAssetComponent[] Components => _components ??= assetId.IsEmpty
        ? []
        : AssetMarshal.TryGetDebugContext(out AssetContext? assetContext)
            ? assetContext.Components(assetId).ToArray()
            : [];

    private IAssetComponent[]? _components;
}