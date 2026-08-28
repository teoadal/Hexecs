namespace Hexecsm;

public sealed partial class World
{
    private uint _nextActorId;

    public ActorId CreateActor()
    {
        ActorId result = ActorId.Unsafe(Interlocked.Increment(ref _nextActorId));

        PostponeOperation(Operation.Add(result));

        return result;
    }

    public void DestroyActor(ActorId actorId)
    {
        PostponeOperation(Operation.Destroy(actorId));
    }
}
