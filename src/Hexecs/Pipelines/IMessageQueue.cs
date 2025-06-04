namespace Hexecs.Pipelines;

/// <summary>
/// Интерфейс очереди сообщений
/// </summary>
public interface IMessageQueue
{
    /// <summary>
    /// Группа, к которой принадлежит очередь сообщений
    /// </summary>
    string Group { get; }

    /// <summary>
    /// Выполняет обработку сообщений в очереди. 
    /// </summary>
    /// <remarks>
    /// На время выполнения сообщений метод <see cref="IMessageQueue{T}.Enqueue"/> блокируется
    /// </remarks>
    void Execute();
}

/// <summary>
/// Обобщенный интерфейс очереди сообщений для определенного типа сообщений
/// </summary>
/// <typeparam name="TMessage">Тип сообщения в очереди</typeparam>
public interface IMessageQueue<TMessage> : IMessageQueue
    where TMessage : struct, IMessage
{
    /// <summary>
    /// Добавляет сообщение в очередь
    /// </summary>
    /// <param name="message">Сообщение для добавления в очередь</param>
    /// <remarks>
    /// Может быть заблокирован при выполнении метода <see cref="IMessageQueue.Execute"/>.
    /// Для большей безопасности можно использовать метод <see cref="TryEnqueue"/> 
    /// </remarks>
    void Enqueue(in TMessage message);
    
    /// <summary>
    /// Пытается добавить сообщение в очередь
    /// </summary>
    /// <param name="message">Сообщение для добавления в очередь</param>
    /// <returns>Возвращает true, если очередь не заблокирована выполнением <see cref="IMessageQueue.Execute"/>; false, если заблокирована</returns>
    bool TryEnqueue(in TMessage message);
}