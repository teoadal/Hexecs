using System.Runtime.InteropServices;
using Hexecs.Benchmarks.Map.Terrains.ValueTypes;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Hexecs.Benchmarks.Map.Terrains;

internal sealed class TerrainTextureStorage(ContentManager content)
{
    private readonly Dictionary<Key, Texture2D> _cache = new();

    public Texture2D GetTexture(TerrainType type, TerrainOverlay overlay)
    {
        var key = new Key(type, overlay);
        ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(_cache, key, out var exists);
        if (exists) return value!;

        value = content.Load<Texture2D>("Content/Textures/Terrain/" + type + "/" + overlay + ".png");
        return value;
    }

    private readonly record struct Key(TerrainType Type, TerrainOverlay Overlay);
}