using Hexecs.Benchmarks.Map.Terrains;

namespace Hexecs.Benchmarks.Map.Common.Positions;

internal sealed class PositionBuilder(TerrainSettings terrainSettings) : IActorBuilder<PositionAbility>
{
    private readonly int _terrainTileSize = terrainSettings.TileSize;

    public void Build(in Actor actor, in AssetRef<PositionAbility> asset, Args args)
    {
        var grid = args.Get<Point>(nameof(Point));
        actor.Add(new Position
        {
            Grid = grid,
            World = new Point(grid.X * _terrainTileSize, grid.Y * _terrainTileSize)
        });
    }
}