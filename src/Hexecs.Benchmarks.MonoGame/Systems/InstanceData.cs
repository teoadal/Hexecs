using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hexecs.Benchmarks.MonoGame.Systems;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct InstanceData : IVertexType
{
    public Vector4 PositionScale; // Будет мапиться на POSITION1
    public Color Color; // Будет мапиться на COLOR0

    // Описываем разметку для GPU
    public static readonly VertexDeclaration VertexDeclaration = new(
        new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 1),
        new VertexElement(16, VertexElementFormat.Color, VertexElementUsage.Color, 0)
    );

    VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
}