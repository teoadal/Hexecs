using System.Text;

namespace Hexecs.Utils;

/// <summary>
/// Утилитарный класс для работы со строками.
/// </summary>
public static class StringUtils
{
    /// <summary>
    /// Значение, представляющее пустую строку.
    /// </summary>
    public const string EmptyValue = "EMPTY";

    private const string Choices = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!?_@#$%";

    [ThreadStatic] private static StringBuilder? _stringBuilder;

    /// <summary>
    /// Преобразует StringBuilder в строку и возвращает его в пул.
    /// </summary>
    /// <param name="builder">Экземпляр StringBuilder для обработки.</param>
    /// <returns>Строка, содержащая текущее значение StringBuilder.</returns>
    public static string Flush(StringBuilder builder)
    {
        var result = builder.ToString();

        Return(builder);

        return result;
    }

    /// <summary>
    /// Генерирует случайную строку указанной длины.
    /// </summary>
    /// <param name="length">Длина генерируемой строки.</param>
    /// <returns>Случайная строка указанной длины.</returns>
    [SkipLocalsInit]
    public static string GetRandom(int length)
    {
        if (length < 257)
        {
            Span<char> stackBuffer = stackalloc char[length];
            Random.Shared.GetItems(Choices, stackBuffer);
            return new string(stackBuffer);
        }

        var array = ArrayPool<char>.Shared.Rent(length);
        var arrayBuffer = array.AsSpan(0, length);
        Random.Shared.GetItems(Choices, arrayBuffer);

        var result = new string(arrayBuffer);
        ArrayPool<char>.Shared.Return(array);

        return result;
    }

    public static void GetRandom(Span<char> buffer) => Random.Shared.GetItems(Choices, buffer);

    /// <summary>
    /// Получает экземпляр StringBuilder из пула или создает новый.
    /// </summary>
    /// <param name="capacity">Начальная емкость StringBuilder.</param>
    /// <returns>Экземпляр StringBuilder.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StringBuilder Rent(int capacity = 2048)
    {
        var builder = _stringBuilder ?? new StringBuilder(capacity);
        builder.EnsureCapacity(capacity);

        return builder;
    }

    /// <summary>
    /// Возвращает экземпляр StringBuilder в пул.
    /// </summary>
    /// <param name="builder">Экземпляр StringBuilder для возврата в пул.</param>
    /// <param name="clear">Флаг, указывающий, нужно ли очищать StringBuilder перед возвратом.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Return(StringBuilder builder, bool clear = true)
    {
        if (clear) builder.Clear();

        _stringBuilder = builder;
    }
}