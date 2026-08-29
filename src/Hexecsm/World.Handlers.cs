namespace Hexecsm;

public sealed partial class World
{
    private void AddHandler(ActorId actorId)
    {
        _storage.TryAdd(actorId, new Entry());
    }

    private void ClearHandler()
    {
        foreach (ref Entry entry in _storage.Values.AsSpan())
        {
            entry.Dispose();
        }

        _storage.Clear();
    }

    private void DestroyHandler(ActorId actorId)
    {
        if (_storage.Remove(actorId, clear: true, out Entry entry))
        {
            entry.Dispose();
            return;
        }

        ThrowActorNotFound(actorId);
    }
}
