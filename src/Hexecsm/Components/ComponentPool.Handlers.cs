using Hexecsm.Accessors;

namespace Hexecsm.Components;

internal sealed partial class ComponentPool<T>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddHandler(ActorId actorId, in T component)
    {
        if (_storage.TryAdd(actorId, in component))
        {
            ProduceAddedEvent(actorId, in component);

            return;
        }

        ThrowAlreadyExists(actorId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ClearHandler()
    {
        if (disposeHandler != null)
        {
            KeyValueAccessor<T> values = _storage.GetAccessor();

            for (var i = 0; i < values.Length; i++)
            {
                KeyValueRef<T> keyValue = values[i];
                disposeHandler.Invoke(keyValue.Key, in keyValue.Value);
            }
        }

        _storage.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CloneHandler(ActorId targetId, in T component)
    {
        if (_storage.TryAdd(targetId, in component))
        {
            ProduceAddedEvent(targetId, in component);

            return;
        }

        ThrowAlreadyExists(targetId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RemoveHandler(ActorId actorId)
    {
        bool hasDisposeHandler = disposeHandler != null;

        if (_storage.Remove(actorId, clear: hasDisposeHandler, out T removed))
        {
            ProduceRemovedEvent(actorId, in removed);
            disposeHandler?.Invoke(actorId, in removed);

            return;
        }

        ThrowComponentNotFound(actorId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateHandler(ActorId actorId, in T expected)
    {
        ref T exists = ref _storage.TryGetRef(actorId);

        if (!Unsafe.IsNullRef(ref exists))
        {
            ProduceUpdatingEvent(actorId, in exists, in expected);

            return;
        }

        ThrowComponentNotFound(actorId);
    }
}
