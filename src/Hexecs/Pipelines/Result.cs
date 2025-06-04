namespace Hexecs.Pipelines;


/// <summary>
/// Структура для представления результата выполнения операции.
/// </summary>
/// <param name="isOk">Флаг успешности выполнения операции.</param>
/// <param name="message">Сообщение с дополнительной информацией о результате.</param>
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly struct Result(bool isOk, string? message)
{
    /// <summary>
    /// Создает результат операции на основе флага успешности.
    /// </summary>
    /// <param name="isOk">Флаг успешности выполнения операции.</param>
    /// <param name="message">Дополнительное сообщение.</param>
    /// <returns>Результат операции.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result Create(bool isOk, string? message = null) => isOk ? Ok(message) : Failed(message);

    /// <summary>
    /// Создает успешный результат операции.
    /// </summary>
    /// <param name="message">Дополнительное сообщение.</param>
    /// <returns>Успешный результат операции.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result Ok(string? message = null) => new(true, message);

    /// <summary>
    /// Создает неуспешный результат операции.
    /// </summary>
    /// <param name="message">Сообщение об ошибке.</param>
    /// <returns>Неуспешный результат операции.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result Failed(string? message = null) => new(false, message ?? "Something went wrong");

    /// <summary>
    /// Флаг, указывающий на успешность выполнения операции.
    /// </summary>
    public readonly bool IsOk = isOk;

    /// <summary>
    /// Сообщение с дополнительной информацией о результате операции.
    /// </summary>
    public readonly string? Message = message;

    /// <summary>
    /// Неявное преобразование результата в логическое значение.
    /// </summary>
    /// <param name="result">Результат операции.</param>
    /// <returns>Значение флага успешности.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator bool(in Result result) => result.IsOk;
    
    /// <summary>
    /// Неявное преобразование логического значения в результат.
    /// </summary>
    /// <param name="isOk">Флаг успешности.</param>
    /// <returns>Результат операции.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result(bool isOk) => Create(isOk);
}