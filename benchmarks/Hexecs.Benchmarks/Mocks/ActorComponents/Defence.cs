using Friflo.Engine.ECS;

namespace Hexecs.Benchmarks.Mocks.ActorComponents;

public struct Defence : IActorComponent, IComponent, Hexecsm.IComponent
{
    public int Value;
}
