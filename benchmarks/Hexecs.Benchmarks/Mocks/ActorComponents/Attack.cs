using Friflo.Engine.ECS;

namespace Hexecs.Benchmarks.Mocks.ActorComponents;

public struct Attack : IActorComponent, IComponent, Hexecsm.IComponent
{
    public int Value;
}
