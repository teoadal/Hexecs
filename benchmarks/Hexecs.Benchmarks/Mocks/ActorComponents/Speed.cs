using Friflo.Engine.ECS;

namespace Hexecs.Benchmarks.Mocks.ActorComponents;

public struct Speed : IActorComponent, IComponent
{
    public int Value;
}