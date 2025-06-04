using Hexecs.Actors.Nodes;

namespace Hexecs.Actors;

public sealed partial class ActorContext
{
    public void AddChild(uint parentId, uint childId)
    {
        var pool = GetOrCreateComponentPool<ActorNodeComponent>();

        ref var child = ref GetOrAddComponent(childId, ActorNodeComponent.Create);
        ref var parent = ref GetOrAddComponent(parentId, ActorNodeComponent.Create);

        parent.Node.AddChild(child.Node);
    }

    public ChildrenEnumerator Children(uint parentId)
    {
        ref var component = ref TryGetComponentRef<ActorNodeComponent>(parentId);
        return Unsafe.IsNullRef(ref component)
            ? ChildrenEnumerator.Empty
            : new ChildrenEnumerator(this, component.Node.AsSpan());
    }

    public bool HasChild(uint parentId, uint childId)
    {
        ref var component = ref TryGetComponentRef<ActorNodeComponent>(parentId);
        return !Unsafe.IsNullRef(ref component) && component.Node.HasChild(childId);
    }

    public bool RemoveChild(uint parentId, uint childId)
    {
        ref var component = ref TryGetComponentRef<ActorNodeComponent>(parentId);
        return !Unsafe.IsNullRef(ref component) && component.Node.RemoveChild(childId);
    }

    public bool TryGetParent(uint childId, out Actor parent)
    {
        ref var component = ref TryGetComponentRef<ActorNodeComponent>(childId);
        if (!Unsafe.IsNullRef(ref component))
        {
            var parentId = component.Node.Parent?.Id;
            if (parentId != null)
            {
                parent = new Actor(this, parentId.Value);
                return true;
            }
        }

        parent = Actor.Empty;
        return false;
    }
}