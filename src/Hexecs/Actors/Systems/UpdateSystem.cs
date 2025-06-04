using Hexecs.Assets;
using Hexecs.Dependencies;
using Hexecs.Loggers;
using Hexecs.Values;
using Hexecs.Worlds;

namespace Hexecs.Actors.Systems;

public abstract class UpdateSystem(ActorContext context) : IUpdateSystem
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

    public abstract void Update(in WorldTime time);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ContextLogger CreateLogger() => Context
        .GetRequiredService<LogService>()
        .CreateContext(GetType());

    ActorContext IUpdateSystem.Context => Context;
}

public abstract class UpdateSystem<T1> : UpdateSystem
    where T1 : struct, IActorComponent
{
    private readonly ActorFilter<T1> _filter;

    protected UpdateSystem(ActorContext context, Action<ActorConstraint.Builder>? constraint = null) : base(context)
    {
        _filter = constraint == null
            ? context.Filter<T1>()
            : context.Filter<T1>(constraint);
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

        foreach (var actor in _filter)
        {
            Update(in actor, time);
        }

        AfterUpdate(in time);
    }

    protected abstract void Update(in ActorRef<T1> actor, in WorldTime time);
}

public abstract class UpdateSystem<T1, T2> : UpdateSystem
    where T1 : struct, IActorComponent
    where T2 : struct, IActorComponent
{
    private readonly ActorFilter<T1, T2> _filter;

    protected UpdateSystem(ActorContext context, Action<ActorConstraint.Builder>? constraint = null) : base(context)
    {
        _filter = constraint == null
            ? context.Filter<T1, T2>()
            : context.Filter<T1, T2>(constraint);
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

        foreach (var actor in _filter)
        {
            Update(in actor, time);
        }

        AfterUpdate(in time);
    }

    protected abstract void Update(in ActorRef<T1, T2> actor, in WorldTime time);
}

public abstract class UpdateSystem<T1, T2, T3> : UpdateSystem
    where T1 : struct, IActorComponent
    where T2 : struct, IActorComponent
    where T3 : struct, IActorComponent
{
    private readonly ActorFilter<T1, T2, T3> _filter;

    protected UpdateSystem(ActorContext context, Action<ActorConstraint.Builder>? constraint = null) : base(context)
    {
        _filter = constraint == null
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

        foreach (var actor in _filter)
        {
            Update(in actor, time);
        }

        AfterUpdate(in time);
    }

    protected abstract void Update(in ActorRef<T1, T2, T3> actor, in WorldTime time);
}