namespace Hexecs.Loggers;

/// <summary>
/// Интерфейс передачи логов в подсистему вывода, например, в консоль.
/// </summary>
public interface ILogSink : IDisposable
{
    /// <summary>
    /// Проверка, является ли уровень логирования доступным для этой подсистемы вывода.
    /// </summary>
    /// <param name="level">Уровень логирования</param>
    /// <returns>Возвращает true, если доступен;false если нет</returns>
    bool IsEnabled(LogLevel level);
    
    /// <summary>
    /// Записать лог в подсистему вывода.
    /// </summary>
    /// <param name="level">Уровень логирования</param>
    /// <param name="context">Контекст логирования</param>
    /// <param name="template">Шаблон сообщения для логирования</param>
    void Write(LogLevel level, string context, string template);

    void Write<T1>(LogLevel level, string context, string template, T1 arg1);

    void Write<T1, T2>(LogLevel level, string context, string template, T1 arg1, T2 arg2);

    void Write<T1, T2, T3>(LogLevel level, string context, string template, T1 arg1, T2 arg2, T3 arg3);

    void Write<T1, T2, T3, T4>(
        LogLevel level,
        string context, string template,
        T1 arg1, T2 arg2, T3 arg3, T4 arg4);

    void Write<T1, T2, T3, T4, T5>(
        LogLevel level,
        string context, string template,
        T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
}