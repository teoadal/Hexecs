namespace Hexecsm.Threading;

public interface IParallelJob
{
    void Execute(int workerIndex, int workersCount);
}
