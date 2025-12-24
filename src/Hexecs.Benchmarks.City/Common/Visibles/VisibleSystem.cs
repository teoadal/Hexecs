using Hexecs.Actors.Systems;
using Hexecs.Benchmarks.Map.Common.Positions;
using Hexecs.Benchmarks.Map.Utils;
using Hexecs.Threading;
using Hexecs.Worlds;

namespace Hexecs.Benchmarks.Map.Common.Visibles;

internal sealed class VisibleSystem : UpdateSystem<Position>
{
    private const int TileSize = TextureStorage.TerrainTileSize;

    private readonly Camera _camera;

    private CameraViewport _currentViewport;

    public VisibleSystem(ActorContext context, Camera camera, IParallelWorker parallelWorker)
        : base(context, parallelWorker: parallelWorker)
    {
        _camera = camera;
    }

    protected override bool BeforeUpdate(in WorldTime time)
    {
        var currentViewport = _camera.Viewport;

        if (currentViewport.Equals(_currentViewport)) return false; // не обновляем, если камера не двигалась

        _currentViewport = currentViewport;

        return true;
    }

    protected override void Update(in ActorRef<Position> actor, in WorldTime time)
    {
        ref readonly var position = ref actor.Component1.World;

        if (_currentViewport.Visible(position.X, position.Y, TileSize, TileSize))
        {
            actor.TryAdd(new Visible());
        }
        else
        {
            actor.Remove<Visible>();
        }
    }
}