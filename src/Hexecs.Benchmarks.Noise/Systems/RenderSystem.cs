using Hexecs.Actors;
using Hexecs.Actors.Systems;
using Hexecs.Benchmarks.Noise.Components;
using Hexecs.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hexecs.Benchmarks.Noise.Systems;

public sealed class RenderSystem : DrawSystem
{
    private readonly ActorFilter<Position, CircleColor> _filter;
    private readonly DynamicVertexBuffer _instanceBuffer;
    private readonly VertexBuffer _geometryBuffer;
    private readonly IndexBuffer _indexBuffer;
    private readonly InstanceData[] _hostBuffer;
    private readonly GraphicsDevice _device;
    private readonly Effect? _shader;
    private readonly Matrix _projection;

    public RenderSystem(ActorContext context, GraphicsDevice device, int maxEntities)
        : base(context)
    {
        _filter = context.Filter<Position, CircleColor>();
        _device = device;
        _hostBuffer = new InstanceData[maxEntities];

        // 1. Буфер инстансов
        _instanceBuffer = new DynamicVertexBuffer(device, typeof(InstanceData), maxEntities, BufferUsage.WriteOnly);

        // 2. Геометрия одного инстанса (квадрат 1x1)
        var vertices = new VertexPositionTexture[]
        {
            new(new Vector3(-1, -1, 0), new Vector2(0, 0)),
            new(new Vector3(1, -1, 0), new Vector2(1, 0)),
            new(new Vector3(-1, 1, 0), new Vector2(0, 1)),
            new(new Vector3(1, 1, 0), new Vector2(1, 1))
        };
        _geometryBuffer = new VertexBuffer(device, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);
        _geometryBuffer.SetData(vertices);

        _indexBuffer = new IndexBuffer(device, IndexElementSize.SixteenBits, 6, BufferUsage.WriteOnly);
        _indexBuffer.SetData(new ushort[] { 0, 1, 2, 2, 1, 3 });

        // 3. Проекция
        _projection = Matrix.CreateOrthographicOffCenter(0, device.Viewport.Width, device.Viewport.Height, 0, 0, 1);

        // 4. Загрузка шейдера
        var shaderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "Instancing.mgfx");
        if (File.Exists(shaderPath))
        {
            var bytecode = File.ReadAllBytes(shaderPath);
            _shader = new Effect(device, bytecode);
        }
        else
        {
            throw new FileNotFoundException("Instancing shader not found", shaderPath);
        }
    }

    public override void Draw(in WorldTime time)
    {
        var count = _filter.Length;
        if (count <= 0) return;

        // Наполняем буфер инстансов данными из HexECS
        var i = 0;
        foreach (var actor in _filter)
        {
            if (i >= _hostBuffer.Length) break;

            ref var data = ref _hostBuffer[i];
            data.PositionScale = new Vector4(actor.Component1.Value.X, actor.Component1.Value.Y, 4.0f, 0f);
            data.Color = actor.Component2.Value;
            i++;
        }

        _instanceBuffer.SetData(_hostBuffer, 0, i, SetDataOptions.Discard);

        if (_shader != null)
        {
            _device.BlendState = BlendState.AlphaBlend;
            _device.RasterizerState = RasterizerState.CullNone;

            _shader.Parameters["Projection"].SetValue(_projection);

            _device.SetVertexBuffers(
                new VertexBufferBinding(_geometryBuffer, 0, 0),
                new VertexBufferBinding(_instanceBuffer, 0, 1)
            );
            _device.Indices = _indexBuffer;

            foreach (var pass in _shader.CurrentTechnique.Passes)
            {
                pass.Apply();
                _device.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 2, i);
            }
        }
    }
}