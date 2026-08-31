using Hexecsm.Worlds;

namespace Hexecsm.Systems;

public interface IDrawSystem
{
    bool Enabled { get; set; }

    void Draw(in WorldTime time);
}
