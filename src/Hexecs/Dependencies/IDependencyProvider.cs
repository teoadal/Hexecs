namespace Hexecs.Dependencies;

/// <summary>
/// Интерфейс, предоставляющий доступ к сервисам в системе внедрения зависимостей.
/// Используется для получения экземпляров сервисов на основе их типов контрактов.
/// </summary>
public interface IDependencyProvider
{
    /// <summary>
    /// Получает экземпляр сервиса указанного типа.
    /// </summary>
    /// <param name="contract">Тип контракта запрашиваемого сервиса.</param>
    /// <returns>Экземпляр сервиса или null, если сервис не найден.</returns>
    object? GetService(Type contract);

    /// <summary>
    /// Получает экземпляр сервиса указанного типа с использованием обобщенного параметра.
    /// </summary>
    /// <typeparam name="TService">Тип запрашиваемого сервиса.</typeparam>
    /// <returns>Экземпляр сервиса или null, если сервис не найден.</returns>
    TService? GetService<TService>() where TService : class;

    /// <summary>
    /// Получает все экземпляры сервисов указанного типа.
    /// </summary>
    /// <typeparam name="TService">Тип запрашиваемых сервисов.</typeparam>
    /// <returns>Массив экземпляров сервисов указанного типа.</returns>
    TService[] GetServices<TService>() where TService : class;
}