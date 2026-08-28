using Hexecsm.Utils;

namespace Hexecsm.Components;

internal sealed partial class ComponentPool<T>
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AddHandler(ActorId actorId, in T component)
    {
        if (_storage.TryAdd(actorId, in component))
        {
            RaiseAddedEvent(actorId, in component);

            return;
        }

        ThrowAlreadyExists(actorId);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void ClearHandler()
    {
        RaiseClearingEvent();

        if (disposeHandler != null)
        {
            ActorDictionary<T>.Accessor values = _storage.GetAccessor();

            foreach (ActorId actorId in _storage.Keys)
            {
                disposeHandler.Invoke(actorId, in values[actorId]);
            }
        }

        _storage.Clear();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void CloneHandler(ActorId targetId, in T component)
    {
        if (_storage.TryAdd(targetId, in component))
        {
            RaiseAddedEvent(targetId, in component);

            return;
        }

        ThrowAlreadyExists(targetId);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void RemoveHandler(ActorId actorId)
    {
        bool hasDisposeHandler = disposeHandler != null;

        if (_storage.Remove(actorId, clear: hasDisposeHandler, out T removed))
        {
            RaiseRemovedEvent(actorId, in removed);
            disposeHandler?.Invoke(actorId, in removed);

            return;
        }

        ThrowComponentNotFound(actorId);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void UpdateHandler(ActorId actorId, in T expected)
    {
        ref T exists = ref _storage.TryGetRef(actorId);

        if (!Unsafe.IsNullRef(ref exists))
        {
            RaiseUpdatingEvent(actorId, in exists, in expected);

            return;
        }

        ThrowComponentNotFound(actorId);
    }
}
