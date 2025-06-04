namespace Hexecs.Pipelines;

/// <summary>
/// Маркерный интерфейс для запросов с обобщенным результатом в конвейере обработки
/// </summary>
/// <typeparam name="TResult">Тип результата запроса</typeparam>
public interface IQuery<TResult>;