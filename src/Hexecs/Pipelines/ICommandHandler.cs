namespace Hexecs.Pipelines;

/// <summary>
/// Базовый интерфейс обработчика команд
/// </summary>
public interface ICommandHandler;

/// <summary>
/// Интерфейс обработчика команд с типизированной командой
/// </summary>
/// <typeparam name="TCommand">Тип команды, которая должна быть структурой и реализовывать <see cref="ICommand{TResult}"/></typeparam>
public interface ICommandHandler<TCommand> : ICommandHandler<TCommand, Result>
    where TCommand : struct, ICommand<Result>;

/// <summary>
/// Интерфейс обработчика команд с типизированной командой и результатом
/// </summary>
/// <typeparam name="TCommand">Тип команды, которая должна быть структурой и реализовывать <see cref="ICommand{TResult}"/></typeparam>
/// <typeparam name="TResult">Тип результата выполнения команды</typeparam>
public interface ICommandHandler<TCommand, out TResult> : ICommandHandler
    where TCommand : struct, ICommand<TResult>
{
    /// <summary>
    /// Обрабатывает команду
    /// </summary>
    /// <param name="command">Команда для обработки</param>
    /// <returns>Результат выполнения команды</returns>
    TResult Handle(in TCommand command);
}