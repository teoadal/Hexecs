using Hexecs.Actors.Pipelines;
using Hexecs.Benchmarks.Map.Common.Positions;
using Hexecs.Benchmarks.Map.Terrains.Assets;
using Hexecs.Benchmarks.Map.Terrains.ValueTypes;
using Hexecs.Benchmarks.Map.Utils;
using Hexecs.Pipelines;

namespace Hexecs.Benchmarks.Map.Terrains.Commands.Generate;

internal sealed class GenerateTerrainHandler : ActorCommandHandler<GenerateTerrainCommand>
{
    private readonly TerrainSettings _settings;

    public GenerateTerrainHandler(
        ActorContext context,
        TerrainSettings settings) : base(context)
    {
        _settings = settings;
    }

    public override Result Handle(in GenerateTerrainCommand terrainCommand)
    {
        var ground = Assets.GetAsset<TerrainAsset>(TerrainAsset.Ground);
        var river = Assets.GetAsset<TerrainAsset>(TerrainAsset.River);
        var urbanConcrete = Assets.GetAsset<TerrainAsset>(TerrainAsset.UrbanConcrete);

        var height = _settings.Height;
        var width = _settings.Width;

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var args = Args.Rent(nameof(Point), new Point(x, y));
                if (x is > 45 and < 55) // river
                {
                    Context.BuildActor<Terrain>(river,
                        args.Set(nameof(Terrain.Elevation), Elevation.FromValue(-10))
                            .Set(nameof(Terrain.Moisture), Moisture.FromValue(35)));
                }
                else if (x < 10 && y < 10) // urban concrete
                {
                    Context.BuildActor<Terrain>(urbanConcrete, args);
                }
                else // just ground
                {
                    Context.BuildActor<Terrain>(ground, args);
                }
            }
        }

        return Result.Ok();
    }
}