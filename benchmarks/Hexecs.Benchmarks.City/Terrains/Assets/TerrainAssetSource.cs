using Hexecs.Assets.Sources;
using Hexecs.Benchmarks.Map.Common.Positions;
using Hexecs.Benchmarks.Map.Terrains.ValueTypes;

namespace Hexecs.Benchmarks.Map.Terrains.Assets;

internal sealed class TerrainAssetSource : IAssetSource
{
    private IAssetLoader _loader = null!;

    public void Load(IAssetLoader loader)
    {
        _loader = loader;

        Create(TerrainAsset.Ground, "Земля", TerrainType.Ground)
            .WithPosition();
        
        Create(TerrainAsset.River, "Река", TerrainType.WaterRiver)
            .WithPosition();
        
        Create(TerrainAsset.UrbanConcrete, "Бетон", TerrainType.UrbanConcrete)
            .WithPosition();
    }

    private AssetConfigurator Create(
        string alias,
        string name,
        TerrainType type)
    {
        return _loader.CreateAsset(alias, new TerrainAsset(name, type));
    }
}