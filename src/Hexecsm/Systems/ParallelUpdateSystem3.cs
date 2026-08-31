using Hexecsm.Accessors;
using Hexecsm.Components;
using Hexecsm.Filters;
using Hexecsm.Threading;
using Hexecsm.Worlds;

namespace Hexecsm.Systems;

public abstract class ParallelUpdateSystem<T1, T2, T3>(World world) : IUpdateSystem, IParallelJob
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
{
    public bool Enabled { get; set; } = true;

    private readonly ComponentPool<T1> _componentPool1 = world.GetOrAddComponentPool<T1>();
    private readonly ComponentPool<T2> _componentPool2 = world.GetOrAddComponentPool<T2>();
    private readonly ComponentPool<T3> _componentPool3 = world.GetOrAddComponentPool<T3>();

    private readonly Filter<T1, T2, T3> _filter = world.GetFilter<T1, T2, T3>();

    private readonly int _degreeOfParallelism = world.ParallelWorker.DegreeOfParallelism;
    private readonly IParallelWorker _parallelWorker = world.ParallelWorker;

    private int _currentLength;
    private WorldTime _currentTime;

    [SkipLocalsInit]
    public void Update(in WorldTime time)
    {
        int length = _filter.Length;

        if (length > 0)
        {
            _currentTime = time;

            if (length >= _degreeOfParallelism)
            {
                _currentLength = length;
                _parallelWorker.Run(this);
            }
            else
            {
                KeyAccessor keys = _filter.Keys;
                ValueAccessor<T1> components1 = _componentPool1.Values;
                ValueAccessor<T2> components2 = _componentPool2.Values;
                ValueAccessor<T3> components3 = _componentPool3.Values;

                Update(keys, in components1, in components2, in components3, in _currentTime);
            }
        }
    }

    [SkipLocalsInit]
    protected abstract void Update(
        KeyAccessor batchKeys,
        in ValueAccessor<T1> components1,
        in ValueAccessor<T2> components2,
        in ValueAccessor<T3> components3,
        in WorldTime worldTime);

    [SkipLocalsInit]
    void IParallelJob.Execute(int workerIndex, int workersCount)
    {
        int baseBatchSize = _currentLength / workersCount;
        int remainder = _currentLength % workersCount;

        int start = workerIndex * baseBatchSize + (workerIndex < remainder
            ? workerIndex
            : remainder);

        int actualBatchSize = baseBatchSize + (workerIndex < remainder ? 1 : 0);

        KeyAccessor keys = _filter.GetKeys(start, actualBatchSize);
        ValueAccessor<T1> components1 = _componentPool1.Values;
        ValueAccessor<T2> components2 = _componentPool2.Values;
        ValueAccessor<T3> components3 = _componentPool3.Values;

        Update(keys, in components1, in components2, in components3, in _currentTime);
    }
}
