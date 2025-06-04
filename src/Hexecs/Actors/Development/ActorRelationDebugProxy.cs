namespace Hexecs.Actors.Development;

[ExcludeFromCodeCoverage]
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
internal sealed class ActorRelationDebugProxy<T1>(ActorRelation<T1> relation)
    where T1 : struct
{
    public readonly T1 Relation = relation.Relation;

    public IActorComponent[] Components => _components ??= _context.ActorAlive(_actorId)
        ? _context.Components(_actorId).ToArray()
        : [];

    private readonly uint _actorId = relation.Id;
    private readonly ActorContext _context = relation.Context;
    private IActorComponent[]? _components;
}