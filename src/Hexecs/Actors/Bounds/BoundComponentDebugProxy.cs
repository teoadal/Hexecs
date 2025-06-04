using Hexecs.Assets;

namespace Hexecs.Actors.Bounds;

[ExcludeFromCodeCoverage]
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
internal sealed class BoundComponentDebugProxy
{
    public readonly IAssetComponent[] Components;

    public BoundComponentDebugProxy(BoundComponent bound)
    {
        if (bound.AssetId == Asset.EmptyId)
        {
            Components = [];
            return;
        }

        Components = AssetMarshal.TryGetDebugContext(out var assetContext)
            ? assetContext.Components(bound.AssetId).ToArray()
            : [];
    }
}