using Hexecs.Assets;

namespace Hexecs.Actors.Bounds;

[ExcludeFromCodeCoverage]
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
internal sealed class BoundComponentDebugProxy
{
    public readonly IAssetComponent[] Components;

    public BoundComponentDebugProxy(BoundComponent bound)
    {
        if (bound.AssetId.IsEmpty)
        {
            Components = [];

            return;
        }

        Components = AssetMarshal.TryGetDebugContext(out AssetContext? assetContext)
            ? assetContext.Components(bound.AssetId).ToArray()
            : [];
    }
}
