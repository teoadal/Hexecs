namespace Hexecs.Benchmarks.Map.Terrains;

public sealed class TerrainSettings
{
    public static TerrainSettings Default { get; } = new() { Width = 256, Height = 256 };

    public const string Key = "Map:Terrain";

    public int Width { get; init; }
    public int Height { get; init; }
}