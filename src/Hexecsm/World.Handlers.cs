namespace Hexecsm;

public sealed partial class World
{
    private void AddHandler(ActorId actorId)
    {
    }

    private void ClearHandler()
    {
        _storage.Clear();
    }

    private void DestroyHandler(ActorId actorId)
    {
        if (_storage.Remove(actorId, true, out Entry entry))
        {
            // do something
        }

        ThrowActorNotFound(actorId);
    }
}
