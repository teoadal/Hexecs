using Hexecs.Benchmarks.Map.Common.Positions;
using Hexecs.Benchmarks.Map.Terrains.Assets;
using Hexecs.Benchmarks.Map.Terrains.Commands.Generate;
using Hexecs.Configurations;
using Hexecs.Dependencies;
using Hexecs.Worlds;
using Microsoft.Xna.Framework.Content;

namespace Hexecs.Benchmarks.Map.Terrains;

internal static class TerrainInstaller
{
    public static ActorContextBuilder AddTerrain(this ActorContextBuilder builder)
    {
        var terrainSettings = builder.World.GetRequiredService<TerrainSettings>();

        builder.CreateBuilder<TerrainBuilder>();

        builder
            .ConfigureComponentPool<Terrain>(terrain => terrain
                .Capacity(terrainSettings.Width * terrainSettings.Height));

        builder.CreateCommandHandler<GenerateTerrainHandler>();

        builder.CreateDrawSystem<TerrainDrawSystem>();
        
        return builder;
    }

    public static WorldBuilder UseTerrain(this WorldBuilder builder)
    {
        builder
            .UseAddAssetSource(new TerrainAssetSource());

        builder
            .UseScoped(ctx => new ActorDictionary<Point, Position>(
                context: ctx.GetRequiredService<ActorContext>(),
                keyExtractor: terrain => terrain.Grid));
        
        builder
            .UseSingleton(ctx => new TerrainSpriteAtlas(
                contentManager: ctx.GetRequiredService<ContentManager>(),
                fileName: "terrain_atlas",
                settings: ctx.GetRequiredService<TerrainSettings>()));

        builder
            .UseSingleton(ctx => ctx
                .GetService<ConfigurationService>()?
                .GetValue<TerrainSettings>(TerrainSettings.Key) ?? TerrainSettings.Default);

        return builder;
    }
}