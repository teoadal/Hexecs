using Hexecs.Actors.Development;
using Hexecs.Actors.Relations;
using Hexecs.Assets;

namespace Hexecs.Actors;

[DebuggerDisplay("{ToString()}")]
[DebuggerTypeProxy(typeof(ActorRefDebugProxy<,>))]
public readonly ref struct ActorRef<T1, T2>
    where T1 : struct, IActorComponent
    where T2 : struct, IActorComponent
{
    /// <summary>
    /// Контекст актёра, управляющий его жизненным циклом и взаимодействием с компонентами.
    /// </summary>
    public readonly ActorContext Context;

    private readonly ref T1 _component1;
    private readonly ref T2 _component2;

    /// <summary>
    /// Уникальный идентификатор актёра.
    /// </summary>
    public readonly uint Id;

    public static ActorRef<T1, T2> Empty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => default;
    }

    public bool Alive
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Context != null && Context.ActorAlive(Id);
    }

    public ref T1 Component1
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _component1;
    }

    public ref T2 Component2
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _component2;
    }

    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Context == null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ActorRef(ActorContext context, uint id, ref T1 component1, ref T2 component2)
    {
        Context = context;

        _component1 = ref component1;
        _component2 = ref component2;

        Id = id;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add<T>(in T component) where T : struct, IActorComponent => Context.AddComponent(Id, in component);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddChild(in Actor child) => Context.AddChild(Id, child.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T AddRelation<T>(in Actor relative, in T relation) where T : struct
    {
        return ref Context.AddRelation(Id, relative.Id, in relation);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Actor<T> As<T>() where T : struct, IActorComponent => Context.GetActor<T>(Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ActorRef<T> AsRef<T>() where T : struct, IActorComponent => Context.GetActorRef<T>(Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ActorContext.ChildrenEnumerator Children() => Context.Children(Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Destroy() => Context.DestroyActor(Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Get<T>() where T : struct, IActorComponent => ref Context.GetComponent<T>(Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Asset GetAsset() => Context.GetBoundAsset(Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T GetRelation<T>(in Actor relative) where T : struct => ref Context.GetRelation<T>(Id, relative.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has<T>() where T : struct, IActorComponent => Context.HasComponent<T>(Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasRelation<T>(in Actor relative) where T : struct => Context.HasRelation<T>(Id, relative.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Is<T>(out Actor<T> actor) where T : struct, IActorComponent => Context.TryGetActor(Id, out actor);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsRef<T>(out ActorRef<T> actor) where T : struct, IActorComponent
    {
        return Context.TryGetActorRef(Id, out actor);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ActorRelationEnumerator<T> Relations<T>() where T : struct => Context.Relations<T>(Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove<T>() where T : struct, IActorComponent => Context.RemoveComponent<T>(Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove<T>(out T component) where T : struct, IActorComponent
    {
        return Context.RemoveComponent(Id, out component);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool RemoveChild(in Actor child) => Context.RemoveChild(Id, child.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool RemoveRelation<T>(in Actor relative) where T : struct => Context.RemoveRelation<T>(Id, relative.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool RemoveRelation<T>(in Actor relative, out T relation) where T : struct
    {
        return Context.RemoveRelation(Id, relative.Id, out relation);
    }

    public override string ToString() => Context == null
        ? StringUtils.EmptyValue
        : Context.GetDescription(Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryAdd<T>(in T component) where T : struct, IActorComponent => Context.TryAdd(Id, in component);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetAsset(out Asset asset) => Context.TryGetBoundAsset(Id, out asset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T TryGetRef<T>() where T : struct, IActorComponent => ref Context.TryGetComponentRef<T>(Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetParent(out Actor parent) => Context.TryGetParent(Id, out parent);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Update<T>(in T component, bool createIfNotExists = true) where T : struct, IActorComponent
    {
        return Context.UpdateComponent(Id, in component, createIfNotExists);
    }

    #region Equality

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(ActorRef<T1, T2> other) => Id == other.Id && Context == other.Context;

    public override bool Equals(object? obj) => obj is Actor other && Id == other.Id;

    public override int GetHashCode() => HashCode.Combine(Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in ActorRef<T1, T2> left, in ActorRef<T1, T2> right) => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in ActorRef<T1, T2> left, in ActorRef<T1, T2> right) => !left.Equals(right);

    #endregion

    #region Implicit

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator bool(in ActorRef<T1, T2> actor) => !actor.IsEmpty;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ActorId(in ActorRef<T1, T2> actor) => new(actor.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ActorId<T1>(in ActorRef<T1, T2> actor) => new(actor.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ActorId<T2>(in ActorRef<T1, T2> actor) => new(actor.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Actor(in ActorRef<T1, T2> actor) => new(actor.Context, actor.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Actor<T1>(in ActorRef<T1, T2> actor) => new(actor.Context, actor.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Actor<T2>(in ActorRef<T1, T2> actor) => new(actor.Context, actor.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ActorRef<T1>(in ActorRef<T1, T2> actor) => new(
        actor.Context,
        actor.Id,
        ref actor._component1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ActorRef<T2>(in ActorRef<T1, T2> actor) => new(
        actor.Context,
        actor.Id,
        ref actor._component2);

    #endregion
}