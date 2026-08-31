using Hexecsm.Systems;

namespace Hexecsm.Worlds;

public sealed partial class World
{
    private IDrawSystem[] _drawSystems;
    private IUpdateSystem[] _updateSystems;

    internal void LoadSystems(IEnumerable<IDrawSystem> drawSystems, IEnumerable<IUpdateSystem> updateSystems)
    {
        _drawSystems = drawSystems.ToArray();
        _updateSystems = updateSystems.ToArray();
    }
}
