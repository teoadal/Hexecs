using Hexecs.Threading;
using Hexecs.Worlds;

namespace Hexecs.Actors.Systems;

public abstract class UpdateSystem<T1> : UpdateSystem, IParallelJob
    where T1 : struct, IActorComponent
{
    public readonly ActorFilter<T1> Filter;

    private readonly IParallelWorker? _parallelWorker;

    private readonly int _degreeOfParallelism;
    private int _currentBatchSize;
    private int _currentLength;
    private WorldTime _currentTime;

    protected UpdateSystem(
        ActorContext context,
        Action<ActorConstraint.Builder>? constraint = null,
        IParallelWorker? parallelWorker = null) : base(context)
    {
        Filter = constraint == null
            ? context.Filter<T1>()
            : context.Filter<T1>(constraint);

        if (parallelWorker != null)
        {
            _parallelWorker = parallelWorker;
            _degreeOfParallelism = parallelWorker.DegreeOfParallelism;
        }
    }

    protected virtual void AfterUpdate(in WorldTime time)
    {
    }

    protected virtual void BeforeUpdate(in WorldTime time)
    {
    }

    public sealed override void Update(in WorldTime time)
    {
        if (Enabled)
        {
            var length = Filter.Length;
            if (length > 0)
            {
                BeforeUpdate(in time);

                if (_parallelWorker == null)
                {
                    foreach (var actor in Filter)
                    {
                        Update(in actor, in time);
                    }
                }
                else
                {
                    _currentTime = time;
                    _currentLength = length;
                    _currentBatchSize = length / _degreeOfParallelism;
                    _parallelWorker.Run(this);
                }

                AfterUpdate(in time);
            }
        }
    }

    protected abstract void Update(in ActorRef<T1> actor, in WorldTime time);

    void IParallelJob.Execute(int workerIndex, int workerCount)
    {
        var start = workerIndex * _currentBatchSize;
        var length = _currentLength;

        if ((uint)start < (uint)length)
        {
            var batch = workerIndex == workerCount
                ? Filter.Skip(start)
                : Filter.Skip(start, _currentBatchSize);

            ref readonly var currentTime = ref _currentTime;
            foreach (var actor in batch)
            {
                Update(in actor, in currentTime);
            }
        }
    }

    ActorContext IParallelJob.Context => Context;
}