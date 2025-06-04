using Hexecs.Assets.Components;
using Hexecs.Worlds;

namespace Hexecs.Assets;

public static class AssetMarshal
{
    public static ushort GetComponentId<T>() where T : struct, IAssetComponent => AssetComponentType<T>.Id;

    public static ushort GetComponentId(Type componentType) => AssetComponentType.GetId(componentType);

    public static Type GetComponentType(ushort componentId) => AssetComponentType.GetType(componentId);

    public static ref T GetMutableComponent<T>(in Asset asset)
        where T : struct, IAssetComponent
    {
        var pool = asset.Context.GetComponentPool<T>();

        var assetId = asset.Id;
        if (pool == null) AssetError.ComponentNotFound<T>(assetId);
        return ref pool.Get(assetId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetDebugContext([NotNullWhen(true)] out AssetContext? assetContext)
    {
        assetContext = WorldDebug.World?.Assets;
        return assetContext != null;
    }
}