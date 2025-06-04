using System.Collections.Frozen;
using Hexecs.Pipelines;
using Hexecs.Pipelines.Commands;
using Hexecs.Pipelines.Messages;
using Hexecs.Pipelines.Notifications;
using Hexecs.Pipelines.Queries;

namespace Hexecs.Actors;

public sealed partial class ActorContext
{
    private ICommandHandler?[] _commands;
    private IQueryHandler?[] _queries;
    private INotificationHandler?[] _notifications;
    private IMessageQueue?[] _messages;
    private FrozenDictionary<string, MessageQueueGroup> _messageGroups;

    /// <summary>
    /// Отправляет запрос и получает результат.
    /// </summary>
    /// <typeparam name="TQuery">Тип запроса.</typeparam>
    /// <typeparam name="TResult">Тип результата.</typeparam>
    /// <param name="query">Запрос для обработки.</param>
    /// <returns>Результат выполнения запроса.</returns>
    /// <exception cref="Exception">Выбрасывается, если обработчик для данного типа запроса не зарегистрирован.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TResult Ask<TQuery, TResult>(in TQuery query)
        where TQuery : struct, IQuery<TResult>
    {
        var handler = GetQueryHandler<TQuery, TResult>();
        return handler.Handle(in query);
    }

    /// <summary>
    /// Выполняет команду и возвращает стандартный результат.
    /// </summary>
    /// <typeparam name="TCommand">Тип команды.</typeparam>
    /// <param name="command">Команда для выполнения.</param>
    /// <returns>Результат выполнения команды.</returns>
    /// <exception cref="Exception">Выбрасывается, если обработчик для данного типа команды не зарегистрирован.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result Execute<TCommand>(in TCommand command)
        where TCommand : struct, ICommand
    {
        var handler = GetCommandHandler<TCommand, Result>();
        return handler.Handle(in command);
    }

    /// <summary>
    /// Выполняет команду и возвращает результат определенного типа.
    /// </summary>
    /// <typeparam name="TCommand">Тип команды.</typeparam>
    /// <typeparam name="TResult">Тип результата.</typeparam>
    /// <param name="command">Команда для выполнения.</param>
    /// <returns>Результат выполнения команды.</returns>
    /// <exception cref="Exception">Выбрасывается, если обработчик для данного типа команды не зарегистрирован.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TResult Execute<TCommand, TResult>(in TCommand command)
        where TCommand : struct, ICommand<TResult>
    {
        var handler = GetCommandHandler<TCommand, TResult>();
        return handler.Handle(in command);
    }

    /// <summary>
    /// Получает обработчик команд указанного типа.
    /// </summary>
    /// <typeparam name="TCommand">Тип команды.</typeparam>
    /// <typeparam name="TResult">Тип результата команды.</typeparam>
    /// <exception cref="Exception">Выбрасывается, если обработчик для данного типа команды не зарегистрирован.</exception>
    public ICommandHandler<TCommand, TResult> GetCommandHandler<TCommand, TResult>()
        where TCommand : struct, ICommand<TResult>
    {
        var id = CommandType<TCommand>.Id;
        if (id < _commands.Length && _commands[id] is ICommandHandler<TCommand, TResult> handler)
        {
            return handler;
        }

        PipelineError.CommandHandlerNotRegistered(typeof(TCommand));
        return null!;
    }

    /// <summary>
    /// Получает обработчик запросов указанного типа.
    /// </summary>
    /// <typeparam name="TQuery">Тип запроса.</typeparam>
    /// <typeparam name="TResult">Тип результата запроса.</typeparam>
    /// <exception cref="Exception">Выбрасывается, если обработчик для данного типа запроса не зарегистрирован.</exception>
    public IQueryHandler<TQuery, TResult> GetQueryHandler<TQuery, TResult>()
        where TQuery : struct, IQuery<TResult>
    {
        var id = QueryType<TQuery>.Id;
        if (id < _queries.Length && _queries[id] is IQueryHandler<TQuery, TResult> handler)
        {
            return handler;
        }

        PipelineError.QueryHandlerNotRegistered(typeof(TQuery));
        return null!;
    }

    /// <summary>
    /// Получает обработчик уведомлений указанного типа.
    /// </summary>
    /// <typeparam name="TNotification">Тип уведомления.</typeparam>
    /// <exception cref="Exception">Выбрасывается, если обработчик для данного типа уведомлений не зарегистрирован.</exception>
    public INotificationHandler<TNotification> GetNotificationHandler<TNotification>()
        where TNotification : struct, INotification
    {
        var id = NotificationType<TNotification>.Id;
        if (id < _notifications.Length && _notifications[id] is INotificationHandler<TNotification> handler)
        {
            return handler;
        }

        PipelineError.NotificationHandlerNotRegistered(typeof(TNotification));
        return null!;
    }

    /// <summary>
    /// Получает очередь сообщений указанного типа.
    /// </summary>
    /// <typeparam name="TMessage">Тип сообщения.</typeparam>
    /// <exception cref="Exception">Выбрасывается, если обработчик для данного типа сообщений не зарегистрирован.</exception>
    public IMessageQueue<TMessage> GetMessageQueue<TMessage>()
        where TMessage : struct, IMessage
    {
        var id = MessageType<TMessage>.Id;
        if (id < _messages.Length && _messages[id] is IMessageQueue<TMessage> queue)
        {
            return queue;
        }

        PipelineError.MessageHandlerNotRegistered(typeof(TMessage));
        return null!;
    }

    /// <summary>
    /// Получает группу очередей сообщений по имени.
    /// </summary>
    /// <param name="queueName">Имя группы очередей.</param>
    /// <returns>Группа очередей сообщений или ошибка, если группа не найдена.</returns>
    public IMessageQueue GetMessageQueueGroup(string queueName)
    {
        return _messageGroups.TryGetValue(queueName, out var messageQueue)
            ? messageQueue
            : PipelineError.QueueNotFound(queueName);
    }

    /// <summary>
    /// Публикует уведомление для обработки.
    /// </summary>
    /// <typeparam name="TNotification">Тип уведомления.</typeparam>
    /// <param name="notification">Уведомление для публикации.</param>
    /// <exception cref="Exception">Выбрасывается, если обработчик для данного типа уведомления не зарегистрирован.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Publish<TNotification>(in TNotification notification)
        where TNotification : struct, INotification
    {
        var handler = GetNotificationHandler<TNotification>();
        handler.Handle(in notification);
    }

    /// <summary>
    /// Отправляет сообщение в соответствующую очередь.
    /// </summary>
    /// <typeparam name="TMessage">Тип сообщения.</typeparam>
    /// <param name="message">Сообщение для отправки.</param>
    /// <exception cref="Exception">Выбрасывается, если обработчик для данного типа сообщения не зарегистрирован.</exception>
    /// <remarks>
    /// Может быть заблокирован при выполнении метода <see cref="IMessageQueue.Execute"/>.
    /// Для большей безопасности можно использовать метод <see cref="TrySend{TMessage}"/>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Send<TMessage>(in TMessage message)
        where TMessage : struct, IMessage
    {
        var queue = GetMessageQueue<TMessage>();
        queue.Enqueue(in message);
    }

    /// <summary>
    /// Пытается отправить сообщение в соответствующую очередь.
    /// </summary>
    /// <typeparam name="TMessage">Тип сообщения.</typeparam>
    /// <param name="message">Сообщение для отправки.</param>
    /// <exception cref="Exception">Выбрасывается, если обработчик для данного типа сообщения не зарегистрирован.</exception>
    /// <returns>Возвращает true, если очередь не заблокирована выполнением <see cref="IMessageQueue.Execute"/>; false, если заблокирована</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TrySend<TMessage>(in TMessage message)
        where TMessage : struct, IMessage
    {
        var queue = GetMessageQueue<TMessage>();
        return queue.TryEnqueue(in message);
    }
    
    internal void LoadPipelines(
        ICommandHandler?[] commands,
        IQueryHandler?[] queries,
        INotificationHandler?[] notifications,
        IMessageQueue?[] messages)
    {
        _commands = commands;
        _queries = queries;
        _notifications = notifications;
        _messages = messages;

        _messageGroups = messages
            .Where(message => message != null)
            .GroupBy(message => message!.Group)
            .ToFrozenDictionary(
                group => group.Key,
                group => new MessageQueueGroup(group.Key, group.ToArray()!),
                ReferenceComparer<string>.Instance);
    }
}