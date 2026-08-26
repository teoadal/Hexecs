namespace Hexecs.Actors.Development;

[ExcludeFromCodeCoverage]
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
internal sealed class ActorIdDebugProxy(ActorId actorId)
{
    public IActorComponent[] Components => _components ??= actorId.IsEmpty
        ? []
        : ActorMarshal.TryGetDebugContext(out var actorContext)
            ? actorContext.Components(actorId).ToArray()
            : [];

    private IActorComponent[]? _components;
}