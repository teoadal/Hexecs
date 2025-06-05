using Hexecs.Assets.Sources;
using Hexecs.Monogame.Features.Healths;

namespace Hexecs.Monogame.Features.Pilots;

internal sealed class PilotAssetSource : IAssetSource
{
    public void Load(IAssetLoader loader)
    {
        Create(loader, PilotAsset.Ace)
            .Health(1000);

        Create(loader, PilotAsset.Common)
            .Health(500);

        Create(loader, PilotAsset.Newbie)
            .Health(100);
    }

    private static AssetConfigurator Create(IAssetLoader loader, string name)
    {
        return loader.CreateAsset(name, new PilotAsset(name));
    }
}