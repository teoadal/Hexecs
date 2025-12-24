using System.Runtime.InteropServices;
using Hexecs.Benchmarks.Map.Terrains.ValueTypes;
using Microsoft.Xna.Framework.Graphics;

namespace Hexecs.Benchmarks.Map.Utils;

internal sealed class TextureStorage : IDisposable
{
    public const int TerrainTileSize = 16; // Размер одного тайла в пикселях (подставьте свой)
    private const int TerrainSpacing = 1; // Между тайлами есть промежуток в 1 пиксель

    private readonly Texture2D _terrainAtlas;
    private readonly Dictionary<TerrainKey, AtlasTexture> _terrainCache;

    private bool _disposed;

    public TextureStorage(GraphicsDevice graphicsDevice)
    {
        var contentRootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content");

        _terrainAtlas = Texture2D.FromFile(
            graphicsDevice,
            Path.Combine(contentRootPath, "roguelikeSheet_transparent.png"));

        _terrainCache = new Dictionary<TerrainKey, AtlasTexture>(256);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _terrainAtlas.Dispose();
        _terrainCache.Clear();
    }

    public ref readonly AtlasTexture GetTerrainTexture(TerrainType type, TerrainOverlay overlay)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var key = new TerrainKey(type, overlay);
        ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(_terrainCache, key, out var exists);
        if (exists) return ref value;

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

        var x = column * (TerrainTileSize + TerrainSpacing);
        var y = row * (TerrainTileSize + TerrainSpacing);
        var sourceRect = new Rectangle(x, y, TerrainTileSize, TerrainTileSize);

        value = new AtlasTexture(_terrainAtlas, sourceRect);
        return ref value;
    }

    private readonly record struct TerrainKey(TerrainType Type, TerrainOverlay Overlay);
}