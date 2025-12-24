using Hexecs.Worlds;

namespace Hexecs.Actors.Systems;

public abstract class DrawSystem<T1> : DrawSystem
    where T1 : struct, IActorComponent
{
    private readonly ActorFilter<T1> _filter;

    protected DrawSystem(ActorContext context, Action<ActorConstraint.Builder>? constraint = null) : base(context)
    {
        _filter = constraint == null
            ? context.Filter<T1>()
            : context.Filter<T1>(constraint);
    }

    protected virtual void AfterDraw(in WorldTime time)
    {
    }

    /// <summary>
    /// Метод запускается до полного обновления
    /// </summary>
    /// <param name="time">Время мира</param>
    /// <returns>Если возвращает false, то обновление не происходит</returns>
    protected virtual bool BeforeDraw(in WorldTime time) => true;

    public sealed override void Draw(in WorldTime time)
    {
        if (!Enabled) return;

        if (BeforeDraw(in time))
        {
            foreach (var actor in _filter)
            {
                Draw(in actor, time);
            }

            AfterDraw(in time);
        }
    }

    protected abstract void Draw(in ActorRef<T1> actor, in WorldTime time);
}