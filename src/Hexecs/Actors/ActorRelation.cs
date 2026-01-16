using Hexecs.Actors.Development;
using Hexecs.Actors.Relations;
using Hexecs.Assets;

namespace Hexecs.Actors;

[DebuggerTypeProxy(typeof(ActorRelationDebugProxy<>))]
public readonly ref struct ActorRelation<T1>
    where T1 : struct
{
    public ActorRelation<T1> Empty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(null!, Actor.EmptyId, ref Unsafe.NullRef<T1>());
    }

    public readonly uint Id;

    public readonly ActorContext Context;

    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Context == null;
    }

    public ref T1 Relation
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _relation;
    }

    private readonly ref T1 _relation;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ActorRelation(ActorContext context, uint id, ref T1 relation)
    {
        Id = id;
        Context = context;

        _relation = ref relation;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetAsset(out Asset asset) => Context.TryGetBoundAsset(Id, out asset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetParent(out Actor parent) => Context.TryGetParent(Id, out parent);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Update<T>(in T component, bool createIfNotExists = true) where T : struct, IActorComponent
    {
        return Context.UpdateComponent(Id, in component, createIfNotExists);
    }

    #region Implicit

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Actor(in ActorRelation<T1> relation) => new(relation.Context, relation.Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator bool(in ActorRelation<T1> relation) => !relation.IsEmpty;

    #endregion
}