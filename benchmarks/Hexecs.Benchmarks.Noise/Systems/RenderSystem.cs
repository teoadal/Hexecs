using Hexecs.Benchmarks.Noise.Components;

using Hexecsm;
using Hexecsm.Accessors;
using Hexecsm.Components;
using Hexecsm.Filters;
using Hexecsm.Systems;
using Hexecsm.Worlds;

using Microsoft.Xna.Framework.Graphics;

namespace Hexecs.Benchmarks.Noise.Systems;

public sealed class RenderSystem : IDrawSystem
{
    public bool Enabled { get; set; } = true;

    private readonly Components<Position> _components1;
    private readonly Components<CircleColor> _components2;
    private readonly Filter<Position, CircleColor> _filter;

    private readonly DynamicVertexBuffer _instanceBuffer;
    private readonly VertexBuffer _geometryBuffer;
    private readonly IndexBuffer _indexBuffer;
    private readonly InstanceData[] _hostBuffer;
    private readonly GraphicsDevice _device;
    private readonly Effect? _shader;
    private readonly Matrix _projection;

    public RenderSystem(World world, GraphicsDevice device, int maxEntities)
    {
        _components1 = world.GetComponents<Position>();
        _components2 = world.GetComponents<CircleColor>();
        _filter = world.GetFilter<Position, CircleColor>();

        _device = device;
        _hostBuffer = new InstanceData[maxEntities];

        // 1. Буфер инстансов
        _instanceBuffer = new DynamicVertexBuffer(device, typeof(InstanceData), maxEntities, BufferUsage.WriteOnly);

        // 2. Геометрия одного инстанса (квадрат 1x1)
        var vertices = new[]
        {
            new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(1, -1, 0), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(-1, 1, 0), new Vector2(0, 1)),
            new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, 1))
        };
        _geometryBuffer = new VertexBuffer(device, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);
        _geometryBuffer.SetData(vertices);

        _indexBuffer = new IndexBuffer(device, IndexElementSize.SixteenBits, 6, BufferUsage.WriteOnly);
        _indexBuffer.SetData(new ushort[] { 0, 1, 2, 2, 1, 3 });

        // 3. Проекция
        _projection = Matrix.CreateOrthographicOffCenter(0, device.Viewport.Width, device.Viewport.Height, 0, 0, 1);

        // 4. Загрузка шейдера
        string shaderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "Instancing.mgfx");

        if (File.Exists(shaderPath))
        {
            byte[] bytecode = File.ReadAllBytes(shaderPath);
            _shader = new Effect(device, bytecode);
        }
        else
        {
            throw new FileNotFoundException("Instancing shader not found", shaderPath);
        }
    }

    public void Draw(in WorldTime time)
    {
        int count = _filter.Length;

        if (count <= 0)
        {
            return;
        }

        ValueAccessor<Position> positions = _components1.GetValues();
        ValueAccessor<CircleColor> colors = _components2.GetValues();

        var i = 0;

        foreach (ActorId actorId in _filter.Keys.AsReadOnlySpan())
        {
            if (i >= _hostBuffer.Length)
            {
                break;
            }

            ref InstanceData data = ref _hostBuffer[i];

            Vector2 position = positions.GetValue(actorId).Value;

            data.PositionScale = new Vector4(position.X, position.Y, 4.0f, 0f);
            data.Color = colors.GetValue(actorId).Value;
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
                new VertexBufferBinding(_instanceBuffer, 0, 1));
            _device.Indices = _indexBuffer;

            foreach (EffectPass pass in _shader.CurrentTechnique.Passes)
            {
                pass.Apply();
                _device.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 2, i);
            }
        }
    }
}
