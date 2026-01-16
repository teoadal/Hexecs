using Friflo.Engine.ECS;

namespace Hexecs.Benchmarks.Mocks.ActorComponents;

public struct EmployeeAgreement : ILinkRelation
{
    public Entity Target;
    public int Salary;

    public Entity GetRelationKey() => Target;
}