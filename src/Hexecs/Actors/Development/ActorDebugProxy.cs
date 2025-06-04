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

[ExcludeFromCodeCoverage]
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
internal sealed class ActorDebugProxy<T1>(Actor<T1> actor)
    where T1: struct, IActorComponent
{
    public T1 Component1
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => actor.Context.GetComponent<T1>(actor.Id);
    }
    
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public IActorComponent[] Components => _components ??= actor.IsEmpty
        ? []
        : actor.Context.Components(actor.Id).ToArray();

    private IActorComponent[]? _components;
}