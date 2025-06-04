using Hexecs.Worlds;

namespace Hexecs.Tests.Worlds;

public sealed class WorldTestFixture: BaseFixture
{
    public readonly World World = new WorldBuilder().Build();
}