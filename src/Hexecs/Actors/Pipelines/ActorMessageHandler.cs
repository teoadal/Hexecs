using Hexecs.Assets;
using Hexecs.Dependencies;
using Hexecs.Loggers;
using Hexecs.Pipelines;
using Hexecs.Values;
using Hexecs.Worlds;

namespace Hexecs.Actors.Pipelines;

public abstract class ActorMessageHandler<TMessage>(ActorContext context) : IMessageHandler<TMessage>
    where TMessage : struct, IMessage
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

    public abstract void Handle(in TMessage message);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ContextLogger CreateLogger() => Context
        .GetRequiredService<LogService>()
        .CreateContext(GetType().Name);
}