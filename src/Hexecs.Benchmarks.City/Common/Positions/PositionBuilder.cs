using Hexecs.Benchmarks.Map.Utils;

namespace Hexecs.Benchmarks.Map.Common.Positions;

internal sealed class PositionBuilder : IActorBuilder<PositionAbility>
{
    private const int TileSize = TextureStorage.TerrainTileSize;

    public void Build(in Actor actor, in AssetRef<PositionAbility> asset, Args args)
    {
        var grid = args.Get<Point>(nameof(Point));
        actor.Add(new Position
        {
            Grid = grid,
            World = new Point(grid.X * TileSize, grid.Y * TileSize)
        });
    }
}