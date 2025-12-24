using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Graphics;

namespace Hexecs.Benchmarks.Map.Utils;

internal readonly struct AtlasTexture(Texture2D texture, Rectangle region)
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Draw(SpriteBatch spriteBatch, Vector2 position) => spriteBatch.Draw(
        texture,
        position,
        region,
        Color.White);
}