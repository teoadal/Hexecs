namespace Hexecs.Threading;

public interface IParallelWorker : IDisposable
{
    int DegreeOfParallelism { get; }
    
    void Run(IParallelJob job);
}