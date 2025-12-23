using Hexecs.Actors.Systems;
using Hexecs.Benchmarks.Map.Common.Positions;
using Hexecs.Benchmarks.Map.Utils;
using Hexecs.Worlds;
using Microsoft.Xna.Framework.Graphics;

namespace Hexecs.Benchmarks.Map.Terrains;

internal sealed class TerrainDrawSystem : DrawSystem<Position, Terrain>
{
    private const int TileSize = 64;

    private readonly Camera2D _camera;
    private readonly TerrainGrid _grid;
    private readonly SpriteBatch _spriteBatch;

    public TerrainDrawSystem(
        Camera2D camera,
        ActorContext context,
        GraphicsDevice graphicsDevice,
        TerrainGrid grid)
        : base(context)
    {
        _camera = camera;
        _grid = grid;
        _spriteBatch = new SpriteBatch(graphicsDevice);
    }

    protected override void BeforeDraw(in WorldTime time)
    {
        _spriteBatch.Begin(
            transformMatrix: _camera.GetTransformationMatrix(),
            samplerState: SamplerState.PointClamp,
            blendState: BlendState.AlphaBlend);
    }

    protected override void Draw(in ActorRef<Position, Terrain> actor, in WorldTime time)
    {
        ref readonly var gridPosition = ref actor.Component1.Grid;
        ref readonly var terrain = ref actor.Component2;

        var drawPosition = new Vector2(
            gridPosition.X * TileSize,
            gridPosition.Y * TileSize);

        Texture2D texture = null!; //TODO: set it!!!
        
        _spriteBatch.Draw(
            texture,
            drawPosition,
            Color.White);
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