using Hexecsm.Systems;

namespace Hexecsm.Worlds;

public sealed partial class World
{
    private IDrawSystem[] _drawSystems;
    private IUpdateSystem[] _updateSystems;

    internal void LoadSystems(IEnumerable<IDrawSystem> drawSystems, IEnumerable<IUpdateSystem> updateSystems)
    {
        _drawSystems = [.. drawSystems];
        _updateSystems = [.. updateSystems];

        if (_updateSystems.Length > 0)
        {
            _parallelWorker.Start();
        }
    }
}
