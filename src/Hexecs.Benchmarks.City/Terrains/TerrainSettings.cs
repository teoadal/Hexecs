namespace Hexecs.Benchmarks.Map.Terrains;

public sealed class TerrainSettings
{
    public const string Key = "Map:Terrain";

    public static readonly TerrainSettings Default = new()
    {
        TileSize = 16,
        TileSpacing = 1,
        Width = 768,
        Height = 768
    };

    public int TileSize { get; init; }

    public int TileSpacing { get; init; }

    public int Width { get; init; }

    public int Height { get; init; }
}