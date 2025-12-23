namespace Hexecs.Benchmarks.Map.Utils;

public static class PointExtensions
{
    public static void GetNeighborPoints(int x, int y, ref Span<Point> neighbors)
    {
        neighbors[0] = new Point(x - 1, y);
        neighbors[1] = new Point(x + 1, y);
        neighbors[2] = new Point(x, y - 1);
        neighbors[3] = new Point(x, y + 1);
    }
}