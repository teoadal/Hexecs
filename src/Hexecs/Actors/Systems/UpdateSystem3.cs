using Hexecs.Threading;
using Hexecs.Worlds;

namespace Hexecs.Actors.Systems;

public abstract class UpdateSystem<T1, T2, T3> : UpdateSystem, IParallelJob
    where T1 : struct, IActorComponent
    where T2 : struct, IActorComponent
    where T3 : struct, IActorComponent
{
    public readonly ActorFilter<T1, T2, T3> Filter;

    private WorldTime _currentTime;
    private readonly IParallelWorker? _parallelWorker;

    protected UpdateSystem(
        ActorContext context,
        Action<ActorConstraint.Builder>? constraint = null,
        IParallelWorker? parallelWorker = null) : base(context)
    {
        _parallelWorker = parallelWorker;
        Filter = constraint == null
            ? context.Filter<T1, T2, T3>()
            : context.Filter<T1, T2, T3>(constraint);
    }

    protected virtual void AfterUpdate(in WorldTime time)
    {
    }

    protected virtual void BeforeUpdate(in WorldTime time)
    {
    }

    public sealed override void Update(in WorldTime time)
    {
        if (!Enabled) return;

        BeforeUpdate(in time);

        if (_parallelWorker == null)
        {
            foreach (var actor in Filter)
            {
                Update(in actor, time);
            }
        }
        else
        {
            _currentTime = time;
            _parallelWorker.Run(this);
        }

        AfterUpdate(in time);
    }

    protected abstract void Update(in ActorRef<T1, T2, T3> actor, in WorldTime time);

    void IParallelJob.Execute(int workerIndex, int workerCount)
    {
        var batchSize = Filter.Length / _parallelWorker!.DegreeOfParallelism;
        var skip = workerIndex * batchSize;
        var batch = Filter.Skip(skip, batchSize);

        foreach (var actor in batch)
        {
            Update(in actor, _currentTime);
        }
    }

    ActorContext IParallelJob.Context => Context;
}