namespace Hexecs.Pipelines;

/// <summary>
/// Базовый интерфейс для обработчиков запросов.
/// </summary>
public interface IQueryHandler;

/// <summary>
/// Интерфейс обработчика запросов, определяющий метод для обработки запроса и возврата результата.
/// </summary>
/// <typeparam name="TQuery">Тип запроса для обработки.</typeparam>
/// <typeparam name="TResult">Тип возвращаемого результата.</typeparam>
public interface IQueryHandler<TQuery, out TResult> : IQueryHandler
    where TQuery : struct, IQuery<TResult>
{
    /// <summary>
    /// Обрабатывает запрос и возвращает результат.
    /// </summary>
    /// <param name="query">Запрос для обработки.</param>
    /// <returns>Результат обработки запроса.</returns>
    TResult Handle(in TQuery query);
}