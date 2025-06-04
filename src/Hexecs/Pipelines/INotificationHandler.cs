namespace Hexecs.Pipelines;

/// <summary>
/// Базовый интерфейс для обработчиков уведомлений.
/// </summary>
public interface INotificationHandler;

/// <summary>
/// Интерфейс обработчика уведомлений, определяющий метод для обработки уведомления определенного типа.
/// </summary>
/// <typeparam name="TNotification">Тип уведомления для обработки.</typeparam>
public interface INotificationHandler<TNotification> : INotificationHandler
    where TNotification : struct
{
    /// <summary>
    /// Обрабатывает уведомление указанного типа.
    /// </summary>
    /// <param name="notification">Уведомление для обработки.</param>
    void Handle(in TNotification notification);
}