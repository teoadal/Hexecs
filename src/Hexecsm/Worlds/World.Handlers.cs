using Hexecsm.Components;

namespace Hexecsm.Worlds;

public sealed partial class World
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ActorAddHandler(ActorId actorId)
    {
        if (_storage.TryAdd(actorId, new Entry()))
        {
            return;
        }

        ThrowAlreadyExists(actorId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ActorDestroyHandler(ActorId actorId)
    {
        if (_storage.Remove(actorId, clear: true, out Entry entry))
        {
            entry.Dispose();

            return;
        }

        ThrowActorNotFound(actorId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ClearHandler()
    {
        ProduceClearingEvent();

        foreach (ref Entry entry in _storage.Values.AsSpan())
        {
            entry.Dispose();
        }

        _storage.Clear();
        _postponedOperations.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ComponentAddedHandler(ActorId actorId, ComponentTypeId componentTypeId)
    {
        ref Entry entry = ref _storage.TryGetRef(actorId);

        if (!Unsafe.IsNullRef(ref entry))
        {
            if (entry.TryAdd(componentTypeId))
            {
                return;
            }

            ThrowComponentAlreadyExists(actorId, componentTypeId);
        }

        ThrowActorNotFound(actorId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ComponentRemovedHandler(ActorId actorId, ComponentTypeId componentTypeId)
    {
        ref Entry entry = ref _storage.TryGetRef(actorId);

        if (!Unsafe.IsNullRef(ref entry))
        {
            if (entry.Remove(componentTypeId))
            {
                return;
            }

            ThrowComponentNotFound(actorId, componentTypeId);
        }

        ThrowActorNotFound(actorId);
    }
}
