namespace Hexecs.Pipelines;

/// <summary>
/// Маркерный интерфейс для обработчиков сообщений в конвейере обработки
/// </summary>
public interface IMessageHandler;

/// <summary>
/// Обобщенный интерфейс для обработчиков сообщений определенного типа в конвейере обработки
/// </summary>
/// <typeparam name="TMessage">Тип обрабатываемого сообщения</typeparam>
public interface IMessageHandler<TMessage> : IMessageHandler
    where TMessage : struct, IMessage
{
    /// <summary>
    /// Обрабатывает сообщение указанного типа
    /// </summary>
    /// <param name="message">Сообщение для обработки</param>
    void Handle(in TMessage message);
}