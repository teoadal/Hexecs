using Hexecs.Benchmarks.Map.Terrains.ValueTypes;
using Hexecs.Benchmarks.Map.Utils.Sprites;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Hexecs.Benchmarks.Map.Terrains;

internal sealed class TerrainSpriteAtlas : SpriteAtlas<Terrain>
{
    public TerrainSpriteAtlas(ContentManager contentManager, string fileName, TerrainSettings settings)
        : base(contentManager, fileName, settings.TileSize, settings.TileSpacing)
    {
    }

    protected override AtlasKey CreateKey(in Terrain key)
    {
        var type = key.Type;

        var column = type switch
        {
            TerrainType.Ground => 6,
            TerrainType.WaterRiver => 3,
            TerrainType.UrbanConcrete => 7,
            _ => 1
        };

        var row = type switch
        {
            TerrainType.Ground => 0,
            TerrainType.WaterRiver => 1,
            TerrainType.UrbanConcrete => 0,
            _ => 1
        };

        return new AtlasKey(column, row);
    }
}