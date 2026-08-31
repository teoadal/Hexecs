using Hexecsm.Accessors;
using Hexecsm.Components;
using Hexecsm.Filters;
using Hexecsm.Threading;
using Hexecsm.Worlds;

namespace Hexecsm.Systems;

public abstract class ParallelUpdateSystem<T1, T2>(World world) : IUpdateSystem, IParallelJob
    where T1 : struct, IComponent
    where T2 : struct, IComponent
{
    public bool Enabled { get; set; } = true;

    private readonly ComponentPool<T1> _componentPool1 = world.GetOrAddComponentPool<T1>();
    private readonly ComponentPool<T2> _componentPool2 = world.GetOrAddComponentPool<T2>();
    private readonly int _degreeOfParallelism = world.ParallelWorker.DegreeOfParallelism;
    private readonly Filter<T1, T2> _filter = world.GetFilter<T1, T2>();
    private readonly IParallelWorker _parallelWorker = world.ParallelWorker;

    private int _currentBatchSize;
    private int _currentLength;
    private WorldTime _currentTime;

    public void Update(in WorldTime time)
    {
        int length = _filter.Length;

        if (length > 0)
        {
            _currentTime = time;
            _currentLength = length;
            _currentBatchSize = length / _degreeOfParallelism;

            _parallelWorker.Run(this);
        }
    }

    protected virtual void Update(
        KeyAccessor batchKeys,
        in ValueAccessor<T1> components1,
        in ValueAccessor<T2> components2,
        in WorldTime worldTime)
    {
        foreach (ActorId actorId in batchKeys.AsReadOnlySpan())
        {
            var actorRef = new ActorRef<T1, T2>(
                id: actorId,
                component1: ref components1.GetValue(actorId),
                component2: ref components2.GetValue(actorId));

            Update(actorRef, worldTime);
        }
    }

    protected virtual void Update(in ActorRef<T1, T2> actorRef, in WorldTime worldTime)
    {
    }

    void IParallelJob.Execute(int workerIndex, int workersCount)
    {
        int start = workerIndex * _currentBatchSize;
        int length = _currentLength;

        if (start < length)
        {
            KeyAccessor keys = _filter.GetKeys(start, _currentBatchSize);
            ValueAccessor<T1> components1 = _componentPool1.Values;
            ValueAccessor<T2> components2 = _componentPool2.Values;

            Update(keys, in components1, in components2, in _currentTime);
        }
    }
}
