namespace Hexecs.Benchmarks.Map.Terrains;

public sealed class TerrainSettings
{
    public static TerrainSettings Default { get; } = new() { Width = 768, Height = 768 };

    public const string Key = "Map:Terrain";

    public int Width { get; init; }
    public int Height { get; init; }
}