using Hexecs.Actors.Nodes;

namespace Hexecs.Actors;

public sealed partial class ActorContext
{
    public void AddChild(uint parentId, uint childId)
    {
        if (parentId == childId) return;

        ref var childNode = ref GetOrAddComponent<ActorNodeComponent>(childId);

        if (childNode.ParentId == parentId) return;

        // Если у ребенка уже есть родитель — отсоединяем
        if (childNode.ParentId != 0)
        {
            RemoveChild(childNode.ParentId, childId);
        }

        ref var parentNode = ref GetOrAddComponent<ActorNodeComponent>(parentId);

        // Вставляем ребенка в начало списка детей родителя
        var oldFirstId = parentNode.FirstChildId;
        childNode.ParentId = parentId;
        childNode.NextSiblingId = oldFirstId;
        childNode.PrevSiblingId = 0;

        if (oldFirstId != 0)
        {
            ref var oldFirstNode = ref GetComponent<ActorNodeComponent>(oldFirstId);
            oldFirstNode.PrevSiblingId = childId;
        }

        parentNode.FirstChildId = childId;
        parentNode.ChildCount++;
    }

    public ChildrenEnumerator Children(uint parentId)
    {
        ref var component = ref TryGetComponentRef<ActorNodeComponent>(parentId);
        return Unsafe.IsNullRef(ref component) || component.FirstChildId == 0
            ? ChildrenEnumerator.Empty
            : new ChildrenEnumerator(this, component.FirstChildId, component.ChildCount);
    }

    public bool HasChild(uint parentId, uint childId)
    {
        ref var component = ref TryGetComponentRef<ActorNodeComponent>(parentId);
        if (Unsafe.IsNullRef(ref component) || component.FirstChildId == 0) return false;

        foreach (var child in Children(parentId))
        {
            if (child.Id == childId) return true;
        }

        return false;
    }

    public bool RemoveChild(uint parentId, uint childId)
    {
        ref var parentNode = ref TryGetComponentRef<ActorNodeComponent>(parentId);
        if (Unsafe.IsNullRef(ref parentNode)) return false;

        ref var childNode = ref TryGetComponentRef<ActorNodeComponent>(childId);
        if (Unsafe.IsNullRef(ref childNode) || childNode.ParentId != parentId) return false;

        // 1. Обновляем ссылку у предыдущего соседа или у родителя
        if (childNode.PrevSiblingId != 0)
        {
            ref var prevNode = ref GetComponent<ActorNodeComponent>(childNode.PrevSiblingId);
            prevNode.NextSiblingId = childNode.NextSiblingId;
        }
        else
        {
            // Это был первый ребенок
            parentNode.FirstChildId = childNode.NextSiblingId;
        }

        // 2. Обновляем ссылку у следующего соседа
        if (childNode.NextSiblingId != 0)
        {
            ref var nextNode = ref GetComponent<ActorNodeComponent>(childNode.NextSiblingId);
            nextNode.PrevSiblingId = childNode.PrevSiblingId;
        }

        // 3. Сбрасываем данные узла
        childNode.ParentId = 0;
        childNode.NextSiblingId = 0;
        childNode.PrevSiblingId = 0;
        parentNode.ChildCount--;

        return true;
    }

    public bool TryGetParent(uint childId, out Actor parent)
    {
        ref var component = ref TryGetComponentRef<ActorNodeComponent>(childId);
        if (!Unsafe.IsNullRef(ref component) && component.ParentId != 0)
        {
            parent = new Actor(this, component.ParentId);
            return true;
        }

        parent = Actor.Empty;
        return false;
    }

    private void OnNodeRemoving(ref ActorNodeComponent node)
    {
        var parentId = node.ParentId;
        if (parentId != 0)
        {
            ref var parentNode = ref TryGetComponentRef<ActorNodeComponent>(parentId);
            if (!Unsafe.IsNullRef(ref parentNode))
            {
                var prevId = node.PrevSiblingId;
                var nextId = node.NextSiblingId;

                if (prevId != 0)
                {
                    ref var prevNode = ref TryGetComponentRef<ActorNodeComponent>(prevId);
                    if (!Unsafe.IsNullRef(ref prevNode)) prevNode.NextSiblingId = nextId;
                }
                else
                {
                    parentNode.FirstChildId = nextId;
                }

                if (nextId != 0)
                {
                    ref var nextNode = ref TryGetComponentRef<ActorNodeComponent>(nextId);
                    if (!Unsafe.IsNullRef(ref nextNode)) nextNode.PrevSiblingId = prevId;
                }

                parentNode.ChildCount--;
            }
        }

        // 2. У всех детей обнуляем ссылки (теперь они сироты без соседей)
        var currentChildId = node.FirstChildId;
        while (currentChildId != 0)
        {
            ref var childNode = ref TryGetComponentRef<ActorNodeComponent>(currentChildId);
            if (Unsafe.IsNullRef(ref childNode)) break;

            var nextSiblingId = childNode.NextSiblingId;

            childNode.ParentId = 0;
            childNode.PrevSiblingId = 0;
            childNode.NextSiblingId = 0;

            currentChildId = nextSiblingId;
        }
    }
}