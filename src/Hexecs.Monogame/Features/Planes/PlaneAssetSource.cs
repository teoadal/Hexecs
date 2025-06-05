using Hexecs.Assets.Sources;
using Hexecs.Monogame.Features.Healths;

namespace Hexecs.Monogame.Features.Planes;

internal sealed class PlaneAssetSource : IAssetSource
{
    public void Load(IAssetLoader loader)
    {
        Create(loader, PlaneAsset.Big)
            .Health(1000);

        Create(loader, PlaneAsset.Common)
            .Health(500);

        Create(loader, PlaneAsset.Small)
            .Health(100);
    }

    private static AssetConfigurator Create(IAssetLoader loader, string name)
    {
        return loader.CreateAsset(name, new PlaneAsset(name));
    }
}