namespace Hexecs.Pipelines;

/// <summary>
/// Класс, содержащий методы для выброса исключений, связанных с ошибками в конвейерах обработки.
/// Все методы этого класса предназначены исключительно для генерации исключений в различных ситуациях ошибок.
/// </summary>
internal static class PipelineError
{
    /// <summary>
    /// Выбрасывает исключение, когда обработчик команды уже зарегистрирован в текущем контексте.
    /// </summary>
    /// <param name="type">Тип команды, для которой обработчик уже зарегистрирован.</param>
    [DoesNotReturn]
    public static void CommandHandlerAlreadyRegistered(Type type)
    {
        throw new Exception($"Command handler for {TypeOf.GetTypeName(type)} already registered in current context");
    }

    [DoesNotReturn]
    public static void CommandHandlerNotImplementedHandlerInterface(Type type)
    {
        throw new Exception($"Type '{TypeOf.GetTypeName(type)}' should implement '{typeof(ICommandHandler<,>)}' interface");
    }
    
    /// <summary>
    /// Выбрасывает исключение, когда обработчик команды не зарегистрирован в текущем контексте.
    /// </summary>
    /// <param name="type">Тип команды, для которой не найден обработчик.</param>
    [DoesNotReturn]
    public static void CommandHandlerNotRegistered(Type type)
    {
        throw new Exception($"Command handler for '{TypeOf.GetTypeName(type)}' isn't registered in current context");
    }

    /// <summary>
    /// Выбрасывает исключение, когда не найден тип команды по указанному идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор типа команды.</param>
    [DoesNotReturn]
    public static void CommandTypeNotFound(uint id)
    {
        throw new Exception($"Command type with id '{id}' isn't found");
    }

    /// <summary>
    /// Выбрасывает исключение, когда обработчик запроса уже зарегистрирован в текущем контексте.
    /// </summary>
    /// <param name="type">Тип запроса, для которого обработчик уже зарегистрирован.</param>
    [DoesNotReturn]
    public static void QueryHandlerAlreadyRegistered(Type type)
    {
        throw new Exception($"Query handler for {TypeOf.GetTypeName(type)} isn't registered in current context");
    }

    /// <summary>
    /// Выбрасывает исключение, когда обработчик запроса не зарегистрирован в текущем контексте.
    /// </summary>
    /// <param name="type">Тип запроса, для которого не найден обработчик.</param>
    [DoesNotReturn]
    public static void QueryHandlerNotRegistered(Type type)
    {
        throw new Exception($"Query handler for {TypeOf.GetTypeName(type)} isn't registered in current context");
    }

    /// <summary>
    /// Выбрасывает исключение, когда не найден тип запроса по указанному идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор типа запроса.</param>
    [DoesNotReturn]
    public static void QueryTypeNotFound(uint id)
    {
        throw new Exception($"Query type with id '{id}' isn't found");
    }

    /// <summary>
    /// Выбрасывает исключение, когда не найдена очередь сообщений с указанным именем.
    /// </summary>
    /// <param name="queueName">Имя очереди сообщений.</param>
    /// <returns>Никогда не возвращает значение, всегда выбрасывает исключение.</returns>
    [DoesNotReturn]
    public static IMessageQueue QueueNotFound(string queueName)
    {
        throw new Exception($"Queue name '{queueName}' isn't registered");
    }

    /// <summary>
    /// Выбрасывает исключение, когда конвейер уведомлений уже зарегистрирован в текущем контексте.
    /// </summary>
    /// <param name="type">Тип уведомления, для которого конвейер уже зарегистрирован.</param>
    [DoesNotReturn]
    public static void NotificationPipelineAlreadyRegistered(Type type)
    {
        throw new Exception(
            $"Notification pipline for {TypeOf.GetTypeName(type)} already registered in current context");
    }

    /// <summary>
    /// Выбрасывает исключение, когда обработчик уведомления не зарегистрирован в текущем контексте.
    /// </summary>
    /// <param name="type">Тип уведомления, для которого не найден обработчик.</param>
    [DoesNotReturn]
    public static void NotificationHandlerNotRegistered(Type type)
    {
        throw new Exception($"Notification handler for {TypeOf.GetTypeName(type)} isn't registered in current context");
    }

    /// <summary>
    /// Выбрасывает исключение, когда не найден тип уведомления по указанному идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор типа уведомления.</param>
    [DoesNotReturn]
    public static void NotificationTypeNotFound(uint id)
    {
        throw new Exception($"Notification type with id '{id}' isn't found");
    }

    /// <summary>
    /// Выбрасывает исключение, когда обработчик сообщения не зарегистрирован в текущем контексте.
    /// </summary>
    /// <param name="type">Тип сообщения, для которого не найден обработчик.</param>
    [DoesNotReturn]
    public static void MessageHandlerNotRegistered(Type type)
    {
        throw new Exception($"Message handler for {TypeOf.GetTypeName(type)} isn't registered in current context");
    }

    /// <summary>
    /// Выбрасывает исключение, когда очередь сообщений уже зарегистрирована в текущем контексте.
    /// </summary>
    /// <param name="type">Тип сообщения, для которого очередь уже зарегистрирована.</param>
    [DoesNotReturn]
    public static void MessageQueueAlreadyRegistered(Type type)
    {
        throw new Exception($"Message queue for {TypeOf.GetTypeName(type)} already registered in current context");
    }

    /// <summary>
    /// Выбрасывает исключение, когда не найден тип сообщения по указанному идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор типа сообщения.</param>
    [DoesNotReturn]
    public static void MessageTypeNotFound(uint id)
    {
        throw new Exception($"Message type with id '{id}' isn't found");
    }
}