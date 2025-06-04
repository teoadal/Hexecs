using Hexecs.Assets;
using Hexecs.Dependencies;
using Hexecs.Loggers;
using Hexecs.Pipelines;
using Hexecs.Values;
using Hexecs.Worlds;

namespace Hexecs.Actors.Pipelines;

/// <summary>
/// Базовый абстрактный класс для обработчиков команд актёра без возвращаемого результата.
/// Реализует интерфейс <see cref="ICommandHandler{TCommand}"/> для обработки команд.
/// </summary>
/// <typeparam name="TCommand">Тип команды, который должен быть структурой и реализовывать интерфейс <see cref="ICommand"/></typeparam>
public abstract class ActorCommandHandler<TCommand>(ActorContext context) : ICommandHandler<TCommand>
    where TCommand : struct, ICommand
{
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

    public abstract Result Handle(in TCommand command);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ContextLogger CreateLogger() => Context
        .GetRequiredService<LogService>()
        .CreateContext(GetType().Name);
}

/// <summary>
/// Базовый абстрактный класс для обработчиков команд актёра с возвращаемым результатом.
/// Реализует интерфейс <see cref="ICommandHandler{TCommand, TResult}"/> для обработки команд.
/// </summary>
/// <typeparam name="TCommand">Тип команды, который должен быть структурой и реализовывать интерфейс <see cref="ICommand{TResult}"/></typeparam>
/// <typeparam name="TResult">Тип результата, возвращаемого после обработки команды</typeparam>
public abstract class ActorCommandHandler<TCommand, TResult>(ActorContext context) : ICommandHandler<TCommand, TResult>
    where TCommand : struct, ICommand<TResult>
{
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

    public abstract TResult Handle(in TCommand command);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ContextLogger CreateLogger() => Context
        .GetRequiredService<LogService>()
        .CreateContext(GetType().Name);
}