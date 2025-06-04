namespace Hexecs.Pipelines;

/// <summary>
/// Маркерный интерфейс для команд с результатом типа Result в конвейере обработки
/// </summary>
public interface ICommand : ICommand<Result>;

/// <summary>
/// Маркерный интерфейс для команд с обобщенным результатом в конвейере обработки
/// </summary>
/// <typeparam name="TResult">Тип результата команды</typeparam>
public interface ICommand<TResult>;