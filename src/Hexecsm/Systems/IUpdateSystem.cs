using Hexecsm.Worlds;

namespace Hexecsm.Systems;

public interface IUpdateSystem
{
    bool Enabled { get; set; }

    void Update(in WorldTime time);
}
