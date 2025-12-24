using Hexecs.Benchmarks.Map.Terrains;
using Hexecs.Dependencies;

namespace Hexecs.Benchmarks.Map.Common.Positions;

internal static class PositionsInstaller
{
    public static ActorContextBuilder AddPositions(this ActorContextBuilder builder)
    {
        var terrainSettings = builder.World.GetRequiredService<TerrainSettings>();

        builder.AddBuilder<PositionBuilder>();

        builder
            .ConfigureComponentPool<Position>(terrain => terrain
                .Capacity(terrainSettings.Width * terrainSettings.Height));

        return builder;
    }
}