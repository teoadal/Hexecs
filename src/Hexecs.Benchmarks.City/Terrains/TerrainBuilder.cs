using Hexecs.Benchmarks.Map.Terrains.Assets;
using Hexecs.Benchmarks.Map.Terrains.ValueTypes;
using Hexecs.Benchmarks.Map.ValueTypes;

namespace Hexecs.Benchmarks.Map.Terrains;

internal sealed class TerrainBuilder : IActorBuilder<TerrainAsset>
{
    public void Build(in Actor actor, in AssetRef<TerrainAsset> asset, Args args)
    {
        ref readonly var assetData = ref asset.Component1;

        actor.Add(new Terrain
        {
            Elevation = args.GetOrDefault(nameof(Terrain.Elevation), Elevation.Default),
            Moisture = args.GetOrDefault(nameof(Terrain.Moisture), Moisture.Default),
            Overlay = TerrainOverlay.None,
            Temperature = args.GetOrDefault(nameof(Terrain.Temperature), Temperature.Default),
            Type = assetData.Type
        });
    }
}