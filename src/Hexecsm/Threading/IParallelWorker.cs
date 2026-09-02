namespace Hexecsm.Threading;

public interface IParallelWorker : IDisposable
{
    int DegreeOfParallelism { get; }

    bool Started { get; }

    void Run(IParallelJob job);

    void Start();
}
