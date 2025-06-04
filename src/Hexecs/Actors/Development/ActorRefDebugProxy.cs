namespace Hexecs.Actors.Development;

[ExcludeFromCodeCoverage]
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
internal sealed class ActorRefDebugProxy<T1>(ActorRef<T1> actor)
    where T1 : struct, IActorComponent
{
    public T1 Component1
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _context.GetComponent<T1>(_actorId);
    }

    public IActorComponent[] Components => _components ??= _context.ActorAlive(_actorId)
        ? _context.Components(_actorId).ToArray()
        : [];

    private readonly uint _actorId = actor.Id;
    private readonly ActorContext _context = actor.Context;
    private IActorComponent[]? _components;
}

[ExcludeFromCodeCoverage]
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
internal sealed class ActorRefDebugProxy<T1, T2>(ActorRef<T1, T2> actor)
    where T1 : struct, IActorComponent
    where T2 : struct, IActorComponent
{
    public T1 Component1
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _context.GetComponent<T1>(_actorId);
    }

    public T2 Component2
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _context.GetComponent<T2>(_actorId);
    }

    public IActorComponent[] Components => _components ??= _context.ActorAlive(_actorId)
        ? _context.Components(_actorId).ToArray()
        : [];

    private readonly uint _actorId = actor.Id;
    private readonly ActorContext _context = actor.Context;
    private IActorComponent[]? _components;
}

[ExcludeFromCodeCoverage]
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
internal sealed class ActorRefDebugProxy<T1, T2, T3>(ActorRef<T1, T2, T3> actor)
    where T1 : struct, IActorComponent
    where T2 : struct, IActorComponent
    where T3 : struct, IActorComponent
{
    public T1 Component1
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _context.GetComponent<T1>(_actorId);
    }

    public T2 Component2
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _context.GetComponent<T2>(_actorId);
    }

    public T3 Component3
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _context.GetComponent<T3>(_actorId);
    }

    public IActorComponent[] Components => _components ??= _context.ActorAlive(_actorId)
        ? _context.Components(_actorId).ToArray()
        : [];

    private readonly uint _actorId = actor.Id;
    private readonly ActorContext _context = actor.Context;
    private IActorComponent[]? _components;
}