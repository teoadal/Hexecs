using Hexecs.Benchmarks.Spawn.Components;

using Hexecsm;
using Hexecsm.Accessors;
using Hexecsm.Filters;
using Hexecsm.Systems;
using Hexecsm.Worlds;

using Microsoft.Xna.Framework.Graphics;

namespace Hexecs.Benchmarks.Spawn.Systems;

public sealed class RenderSystem : IDrawSystem
{
    public bool Enabled { get; set; } = true;

    private readonly World _context;
    private readonly Filter<Position, CircleColor, Lifetime> _filter;

    private readonly DynamicVertexBuffer _instanceBuffer;
    private readonly VertexBuffer _geometryBuffer;
    private readonly IndexBuffer _indexBuffer;
    private readonly InstanceData[] _hostBuffer;
    private readonly GraphicsDevice _device;
    private readonly Effect? _shader;
    private readonly Matrix _projection;

    public RenderSystem(World context, GraphicsDevice device, int maxEntities)
    {
        _filter = context.GetFilter<Position, CircleColor, Lifetime>();
        _context = context;
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

    [SkipLocalsInit]
    public void Draw(in WorldTime time)
    {
        int count = _filter.Length;

        if (count <= 0)
        {
            return;
        }

        ValueAccessor<Position> positions = _context.GetComponents<Position>();
        ValueAccessor<CircleColor> colors = _context.GetComponents<CircleColor>();
        ValueAccessor<Lifetime> lifetimes = _context.GetComponents<Lifetime>();

        var i = 0;

        foreach (ActorId actorId in _filter.Keys.AsReadOnlySpan())
        {
            if (i >= _hostBuffer.Length)
            {
                break;
            }

            ref InstanceData data = ref _hostBuffer[i];

            Vector2 position = positions.GetValue(actorId).Value;
            float lifetime = lifetimes.GetValue(actorId).Value;

            // Рассчитываем размер: от 1.0 до 6.0 пикселей в зависимости от возраста частицы
            float currentScale = MathHelper.Lerp(1.0f, 6.0f, MathHelper.Clamp(lifetime / 4.5f, 0f, 1f));

            data.PositionScale = new Vector4(position.X, position.Y, currentScale, 0f);
            data.Color = colors.GetValue(actorId).Value;
            i++;
        }

        _instanceBuffer.SetData(_hostBuffer, 0, i, SetDataOptions.Discard);

        if (_shader != null)
        {
            _device.BlendState = BlendState.Additive;
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
