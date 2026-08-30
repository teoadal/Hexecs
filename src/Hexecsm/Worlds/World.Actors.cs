using System.Collections.Concurrent;

namespace Hexecsm.Worlds;

public sealed partial class World
{
    private readonly ConcurrentQueue<ActorId> _freeIds = [];
    private uint _nextActorId;

    public ActorId CreateActor()
    {
        if (!_freeIds.TryDequeue(out ActorId actorId))
        {
            actorId = ActorId.Unsafe(Interlocked.Increment(ref _nextActorId));
        }

        PostponeOperation(Operation.Add(actorId));

        return actorId;
    }

    public bool IsAlive(ActorId actorId)
    {
        return _storage.Contains(actorId);
    }

    public void DestroyActor(ActorId actorId)
    {
        PostponeOperation(Operation.Destroy(actorId));
    }
}
