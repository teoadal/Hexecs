namespace Hexecs.Actors.Development;

[ExcludeFromCodeCoverage]
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
internal sealed class ActorIdDebugProxy(ActorId actorId)
{
    public IActorComponent[] Components => _components ??= actorId.IsEmpty
        ? []
        : ActorMarshal.TryGetDebugContext(out var actorContext)
            ? actorContext.Components(actorId.Value).ToArray()
            : [];

    private IActorComponent[]? _components;
}

[ExcludeFromCodeCoverage]
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
internal sealed class ActorIdDebugProxy<T1>(ActorId<T1> actorId)
    where T1 : struct, IActorComponent
{
    public IActorComponent[] Components => _components ??= actorId.IsEmpty
        ? []
        : ActorMarshal.TryGetDebugContext(out var actorContext)
            ? actorContext.Components(actorId.Value).ToArray()
            : [];

    private IActorComponent[]? _components;
}