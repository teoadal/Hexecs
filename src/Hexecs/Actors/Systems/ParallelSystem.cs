using Hexecs.Threading;
using Hexecs.Worlds;

namespace Hexecs.Actors.Systems;

internal sealed class ParallelSystem : IUpdateSystem, IHaveOrder, IParallelJob
{
    public readonly ActorContext Context;

    public bool Enabled { get; set; } = true;

    public int Order { get; }

    private readonly int _batchSize;
    private readonly IUpdateSystem[] _systems;
    private readonly IParallelWorker _worker;

    private WorldTime _currentTime;

    public ParallelSystem(
        ActorContext context,
        int order,
        IUpdateSystem[] systems,
        IParallelWorker worker)
    {
        Context = context;
        Order = order;

        _batchSize = systems.Length / worker.DegreeOfParallelism;
        _systems = systems;
        _worker = worker;
    }

    public void Execute(int workerIndex, int workerCount)
    {
        var skip = workerIndex * _batchSize;
        var batch = _systems.AsSpan(skip, _batchSize);

        foreach (var updateSystem in batch)
        {
            updateSystem.Update(_currentTime);
        }
    }

    public bool TryGetSystem<T>([NotNullWhen(true)] out T? system)
        where T : class, IUpdateSystem
    {
        foreach (var exists in _systems)
        {
            if (exists is not T expected) continue;

            system = expected;
            return true;
        }

        system = null;
        return false;
    }

    public void Update(in WorldTime time)
    {
        if (Enabled)
        {
            _currentTime = time;
            _worker.Run(this);
        }
    }

    ActorContext IUpdateSystem.Context => Context;
    ActorContext IParallelJob.Context => Context;
}