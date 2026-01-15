using Friflo.Engine.ECS;

namespace Hexecs.Benchmarks.Mocks.ActorComponents;

public struct Defence : IActorComponent, IComponent
{
    public int Value;
}