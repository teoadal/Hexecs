using Hexecsm.Components;

namespace Hexecsm;

public sealed partial class World
{
    public void AddComponent<T>(ActorId actorId, in T component)
        where T : struct, IComponent
    {
        ComponentPool<T> componentPool = GetOrAddComponentPool<T>();
        componentPool.Add(actorId, in component);
    }

    public ref T GetComponent<T>(ActorId actorId)
        where T : struct, IComponent
    {
        ComponentPool<T>? componentPool = GetComponentPool<T>();

        if (componentPool != null)
        {
            ref T component = ref componentPool.GetRef(actorId);

            if (!Unsafe.IsNullRef<T>(ref component))
            {
                return ref component;
            }
        }

        ThrowComponentNotFound<T>(actorId);

        return ref Unsafe.NullRef<T>();
    }

    public void RemoveComponent<T>(ActorId actorId)
        where T : struct, IComponent
    {
        ComponentPool<T>? componentPool = GetComponentPool<T>();

        if (componentPool != null)
        {
            componentPool.Remove(actorId);

            return;
        }

        ThrowComponentNotFound<T>(actorId);
    }

    public bool RemoveComponent<T>(ActorId actorId, out T component)
        where T : struct, IComponent
    {
        ComponentPool<T>? componentPool = GetComponentPool<T>();

        if (componentPool != null)
        {
            return componentPool.Remove(actorId, out component);
        }

        ThrowComponentNotFound<T>(actorId);
        component = default;

        return false;
    }
}
