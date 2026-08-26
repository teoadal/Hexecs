namespace Hexecs.Actors.Development;

[ExcludeFromCodeCoverage]
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
internal sealed class ActorDebugProxy(Actor actor)
{
    public IActorComponent[] Components => _components ??= actor.IsEmpty
        ? []
        : actor.Context.Components(actor.Id).ToArray();

    private IActorComponent[]? _components;
}