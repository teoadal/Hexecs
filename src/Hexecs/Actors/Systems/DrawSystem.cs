using Hexecs.Assets;
using Hexecs.Dependencies;
using Hexecs.Loggers;
using Hexecs.Values;
using Hexecs.Worlds;

namespace Hexecs.Actors.Systems;

public abstract class DrawSystem(ActorContext context) : IDrawSystem, IDisposable
{
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Контекст ассетов из контекста мира.
    /// </summary>
    protected readonly AssetContext Assets = context.World.Assets;

    /// <summary>
    /// Контекст актёра, содержащий ссылки на основные сервисы и компоненты системы.
    /// </summary>
    protected readonly ActorContext Context = context;

    /// <summary>
    /// Логгер для текущего контекста.
    /// </summary>
    /// <remarks>
    /// Создается лениво при первом обращении.
    /// </remarks>
    protected ContextLogger Log
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _logger ??= CreateLogger();
    }

    /// <summary>
    /// Сервис для работы со значениями в системе.
    /// </summary>
    protected readonly ValueService Values = context.World.Values;

    /// <summary>
    /// Мир из контекста актёра.
    /// </summary>
    protected readonly World World = context.World;

    private ContextLogger? _logger;

    public abstract void Draw(in WorldTime time);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ContextLogger CreateLogger() => Context
        .GetRequiredService<LogService>()
        .CreateContext(GetType());

    ActorContext IDrawSystem.Context => Context;

    public virtual void Dispose()
    {
    }
}

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

    protected virtual void BeforeDraw(in WorldTime time)
    {
    }

    public sealed override void Draw(in WorldTime time)
    {
        if (!Enabled) return;

        BeforeDraw(in time);

        foreach (var actor in _filter)
        {
            Draw(in actor, time);
        }

        AfterDraw(in time);
    }

    protected abstract void Draw(in ActorRef<T1> actor, in WorldTime time);
}

public abstract class DrawSystem<T1, T2> : DrawSystem
    where T1 : struct, IActorComponent
    where T2 : struct, IActorComponent
{
    private readonly ActorFilter<T1, T2> _filter;

    protected DrawSystem(ActorContext context, Action<ActorConstraint.Builder>? constraint = null) : base(context)
    {
        _filter = constraint == null
            ? context.Filter<T1, T2>()
            : context.Filter<T1, T2>(constraint);
    }

    protected virtual void AfterDraw(in WorldTime time)
    {
    }

    protected virtual void BeforeDraw(in WorldTime time)
    {
    }

    public sealed override void Draw(in WorldTime time)
    {
        if (!Enabled) return;

        BeforeDraw(in time);

        foreach (var actor in _filter)
        {
            Draw(in actor, time);
        }

        AfterDraw(in time);
    }

    protected abstract void Draw(in ActorRef<T1, T2> actor, in WorldTime time);
}

public abstract class DrawSystem<T1, T2, T3> : DrawSystem
    where T1 : struct, IActorComponent
    where T2 : struct, IActorComponent
    where T3 : struct, IActorComponent
{
    private readonly ActorFilter<T1, T2, T3> _filter;

    protected DrawSystem(ActorContext context, Action<ActorConstraint.Builder>? constraint = null) : base(context)
    {
        _filter = constraint == null
            ? context.Filter<T1, T2, T3>()
            : context.Filter<T1, T2, T3>(constraint);
    }

    protected virtual void AfterDraw(in WorldTime time)
    {
    }

    protected virtual void BeforeDraw(in WorldTime time)
    {
    }

    public sealed override void Draw(in WorldTime time)
    {
        if (!Enabled) return;

        BeforeDraw(in time);

        foreach (var actor in _filter)
        {
            Draw(in actor, time);
        }

        AfterDraw(in time);
    }

    protected abstract void Draw(in ActorRef<T1, T2, T3> actor, in WorldTime time);
}