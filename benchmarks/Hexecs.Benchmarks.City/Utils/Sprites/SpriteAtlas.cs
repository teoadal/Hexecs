using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Hexecs.Benchmarks.Map.Utils.Sprites;

internal abstract class SpriteAtlas<TKey> : IDisposable
    where TKey : struct
{
    private readonly Dictionary<TKey, Sprite> _sprites = new();
    private readonly Texture2D _texture;

    private readonly int _tileSize;
    private readonly int _tileSpacing;

    private bool _disposed;

    protected SpriteAtlas(ContentManager contentManager, string fileName, int tileSize, int tileSpacing)
    {
        _texture = contentManager.Load<Texture2D>(fileName);
        _tileSize = tileSize;
        _tileSpacing = tileSpacing;
    }

    public ref Sprite GetSprite(in TKey key)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var atlasKey = CreateKey(in key);
        ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(_sprites, key, out var exists);
        if (exists)
        {
            return ref value;
        }

        value = CreateSprite(atlasKey.Column, atlasKey.Row);
        return ref value;
    }

    protected abstract AtlasKey CreateKey(in TKey key);

    private Sprite CreateSprite(int column, int row)
    {
        var x = column * (_tileSize + _tileSpacing);
        var y = row * (_tileSize + _tileSpacing);
        var sourceRect = new Rectangle(x, y, _tileSize, _tileSize);

        return new Sprite(_texture, sourceRect);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _sprites.Clear();
        _texture.Dispose();
    }

    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected readonly record struct AtlasKey(int Column, int Row);
}