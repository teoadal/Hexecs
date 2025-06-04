using Hexecs.Actors;

namespace Hexecs.Threading;

public interface IParallelJob
{
    ActorContext Context { get; }

    void Execute(int workerIndex, int workerCount);
}