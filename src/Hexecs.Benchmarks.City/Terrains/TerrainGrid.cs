using Hexecs.Benchmarks.Map.Common.Positions;
using Microsoft.Xna.Framework.Graphics;

namespace Hexecs.Benchmarks.Map.Terrains;

internal sealed class TerrainGrid
{
    private ActorContext _context;

    public TerrainGrid(ActorContext context)
    {
        _context = context;
        _context.OnComponentAdded<Terrain>(OnAdded);
        _context.OnComponentRemoving<Terrain>(OnRemoving);
        _context.OnComponentUpdating<Terrain>(OnUpdating);
    }

    public ReadOnlySpan<Entry>.Enumerator GetEnumerator()
    {
        throw new NotImplementedException();
    }

    private void OnAdded(uint actorId, int index, ref Terrain component)
    {
    }

    private void OnRemoving(uint actorId, ref Terrain component)
    {
        throw new NotImplementedException();
    }

    private void OnUpdating(uint actorId, ref Terrain exists, in Terrain expected)
    {
        throw new NotImplementedException();
    }

    public struct Entry
    {
        public Position Position;
        public Terrain Terrain;
        public Texture2D Texture;
    }
}