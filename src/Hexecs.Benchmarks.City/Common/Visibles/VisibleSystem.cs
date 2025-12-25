using Hexecs.Actors.Systems;
using Hexecs.Benchmarks.Map.Common.Positions;
using Hexecs.Benchmarks.Map.Terrains;
using Hexecs.Benchmarks.Map.Utils;
using Hexecs.Benchmarks.Map.Utils.Sprites;
using Hexecs.Threading;
using Hexecs.Worlds;

namespace Hexecs.Benchmarks.Map.Common.Visibles;

internal sealed class VisibleSystem : UpdateSystem<Position>
{
    private readonly Camera _camera;
    private readonly int _tileSize;

    private CameraViewport _currentViewport;

    public VisibleSystem(ActorContext context, Camera camera, IParallelWorker parallelWorker, TerrainSettings settings)
        : base(context, parallelWorker: parallelWorker)
    {
        _camera = camera;
        _tileSize = settings.TileSize;
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

        if (_currentViewport.Visible(position.X, position.Y, _tileSize, _tileSize))
        {
            actor.TryAdd(new Visible());
        }
        else
        {
            actor.Remove<Visible>();
        }
    }
}