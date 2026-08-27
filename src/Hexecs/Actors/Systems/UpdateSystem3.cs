using Hexecs.Threading;
using Hexecs.Worlds;

namespace Hexecs.Actors.Systems;

public abstract class UpdateSystem<T1, T2, T3> : UpdateSystem, IParallelJob
    where T1 : struct, IActorComponent
    where T2 : struct, IActorComponent
    where T3 : struct, IActorComponent
{
    public readonly ActorFilter<T1, T2, T3> Filter;

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
        _parallelWorker = parallelWorker;
        Filter = constraint == null
            ? context.Filter<T1, T2, T3>()
            : context.Filter<T1, T2, T3>(constraint);

        if (parallelWorker != null)
        {
            _parallelWorker = parallelWorker;
            _degreeOfParallelism = parallelWorker.DegreeOfParallelism;
        }
    }

    protected virtual void AfterUpdate(in WorldTime time)
    {
    }

    /// <summary>
    /// Метод запускается до полного обновления
    /// </summary>
    /// <param name="time">Время мира</param>
    /// <returns>Если возвращает false, то обновление не происходит</returns>
    protected virtual bool BeforeUpdate(in WorldTime time)
    {
        return true;
    }

    public sealed override void Update(in WorldTime time)
    {
        if (Enabled)
        {
            int length = Filter.Length;

            if (length > 0)
            {
                if (!BeforeUpdate(in time))
                {
                    return;
                }

                if (_parallelWorker == null)
                {
                    foreach (ActorRef<T1, T2, T3> actor in Filter)
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

    protected abstract void Update(in ActorRef<T1, T2, T3> actor, in WorldTime time);

    void IParallelJob.Execute(int workerIndex, int workersCount)
    {
        int start = workerIndex * _currentBatchSize;
        int length = _currentLength;

        if ((uint)start < (uint)length)
        {
            ActorFilter<T1, T2, T3>.SkipTakeEnumerator batch = workerIndex == workersCount
                ? Filter.Skip(start)
                : Filter.Skip(start, _currentBatchSize);

            ref readonly WorldTime currentTime = ref _currentTime;

            foreach (ActorRef<T1, T2, T3> actor in batch)
            {
                Update(in actor, in currentTime);
            }
        }
    }

    ActorContext IParallelJob.Context => Context;
}
