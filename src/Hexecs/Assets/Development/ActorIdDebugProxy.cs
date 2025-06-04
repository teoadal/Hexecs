using Hexecs.Actors;

namespace Hexecs.Assets.Development;

[ExcludeFromCodeCoverage]
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
internal sealed class AssetIdDebugProxy(AssetId assetId)
{
    public IAssetComponent[] Components => _components ??= assetId.IsEmpty
        ? []
        : AssetMarshal.TryGetDebugContext(out var assetContext)
            ? assetContext.Components(assetId.Value).ToArray()
            : [];

    private IAssetComponent[]? _components;
}

[ExcludeFromCodeCoverage]
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
internal sealed class AssetIdDebugProxy<T1>(ActorId<T1> actorId)
    where T1 : struct, IActorComponent
{
    public IAssetComponent[] Components => _components ??= actorId.IsEmpty
        ? []
        : AssetMarshal.TryGetDebugContext(out var assetContext)
            ? assetContext.Components(actorId.Value).ToArray()
            : [];

    private IAssetComponent[]? _components;
}