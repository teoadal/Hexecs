using Hexecs.Actors.Systems;
using Hexecs.Benchmarks.Map.Common.Positions;
using Hexecs.Benchmarks.Map.Common.Visibles;
using Hexecs.Benchmarks.Map.Utils;
using Hexecs.Worlds;
using Microsoft.Xna.Framework.Graphics;

namespace Hexecs.Benchmarks.Map.Terrains;

internal sealed class TerrainDrawSystem : DrawSystem<Position, Terrain>
{
    private readonly Camera _camera;
    private readonly SpriteBatch _spriteBatch;
    private readonly TextureStorage _textureStorage;

    public TerrainDrawSystem(
        Camera camera,
        ActorContext context,
        GraphicsDevice graphicsDevice,
        TextureStorage textureStorage)
        : base(context, constraint => constraint.Include<Visible>())
    {
        _camera = camera;
        _textureStorage = textureStorage;
        _spriteBatch = new SpriteBatch(graphicsDevice);
    }

    protected override bool BeforeDraw(in WorldTime time)
    {
        _spriteBatch.Begin(
            transformMatrix: _camera.GetTransformationMatrix(),
            samplerState: SamplerState.PointClamp,
            blendState: BlendState.AlphaBlend);

        return true;
    }

    protected override void Draw(in ActorRef<Position, Terrain> actor, in WorldTime time)
    {
        ref readonly var terrain = ref actor.Component2;
        ref readonly var texture = ref _textureStorage.GetTerrainTexture(terrain.Type, terrain.Overlay);

        ref readonly var worldPosition = ref actor.Component1.World;
        texture.Draw(_spriteBatch, new Vector2(worldPosition.X, worldPosition.Y));
    }

    protected override void AfterDraw(in WorldTime time)
    {
        _spriteBatch.End();
    }

    public override void Dispose()
    {
        _spriteBatch.Dispose();
        base.Dispose();
    }
}