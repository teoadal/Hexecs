using System.Globalization;
using System.Numerics;

namespace Hexecs.Utils;

/// <summary>
/// Представляет денежную сумму с точностью до двух знаков после запятой.
/// Внутренне хранит значение в наименьших единицах валюты (копейках).
/// Обеспечивает арифметические операции, сравнение и преобразование между различными числовыми форматами.
/// </summary>
/// <remarks>
/// Денежные значения хранятся как целое число (<see cref="long"/>), представляющее сотые доли (копейки),
/// что позволяет избежать проблем с точностью при использовании чисел с плавающей точкой.
/// </remarks>
[DebuggerDisplay("{ToString()}")]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly struct Money(long value) :
    IComparable<Money>, IEquatable<Money>,
    IMinMaxValue<Money>,
    ISpanFormattable, IUtf8SpanFormattable
{
    /// <summary>
    /// Максимальное значение
    /// </summary>
    public static readonly Money MaxValue = Create(long.MaxValue / 100L - 1, 99);

    /// <summary>
    /// Минимальное значение
    /// </summary>
    public static readonly Money MinValue = Create(long.MinValue / 100L + 1, 99);

    /// <summary>
    /// Создает экземпляр структуры Money с указанными значениями целой и дробной части.
    /// </summary>
    /// <param name="whole">Целая часть суммы денег.</param>
    /// <param name="fraction">Дробная часть суммы денег (от 0 до 99).</param>
    /// <returns>Новый экземпляр структуры Money.</returns>
    /// <exception cref="OverflowException">Если дробная часть не находится в диапазоне от 0 до 99.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Money Create(long whole, int? fraction = null)
    {
        var result = whole * 100;

        if (fraction is < 0 or > 99) ThrowOverflow();

        // ReSharper disable once InvertIf
        if (fraction.HasValue)
        {
            if (whole >= 0) result += fraction.Value;
            else result -= fraction.Value;
        }

        return new Money(result);
    }

    /// <summary>
    /// Пытается преобразовать строковое представление суммы денег в эквивалентный экземпляр структуры Money.
    /// </summary>
    /// <param name="s">Строка, содержащая сумму денег для преобразования.</param>
    /// <param name="result">При успешном выполнении содержит значение типа Money, эквивалентное строке s.</param>
    /// <returns>True, если s успешно преобразована; иначе false.</returns>
    public static bool TryParse(ReadOnlySpan<char> s, out Money result)
    {
        if (double.TryParse(s, CultureInfo.InvariantCulture, out var doubleValue))
        {
            result = new Money((long)(doubleValue * 100));
            return true;
        }

        result = Zero;
        return false;
    }

    /// <summary>
    /// Получает экземпляр структуры Money со значением ноль.
    /// </summary>
    public static Money Zero
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(0);
    }

    /// <summary>
    /// Дробная часть
    /// </summary>
    /// <remarks>
    /// Это остаток от деления на 100
    /// </remarks>
    public int Fraction
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            var fraction = Value % 100;
            return Value >= 0 ? (int)fraction : (int)-fraction;
        }
    }

    /// <summary>
    /// Целая часть
    /// </summary>
    /// <remarks>
    /// Это результат целочисленного деления на 100
    /// </remarks>
    public long Whole
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Value / 100;
    }

    /// <summary>
    /// Внутреннее значение, представляющее деньги в наименьших единицах (копейках).
    /// </summary>
    public readonly long Value = value;

    /// <summary>
    /// Возвращает абсолютное значение суммы.
    /// </summary>
    /// <returns>Абсолютное значение текущей суммы.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Money Abs() => new(Math.Abs(Value));

    /// <summary>
    /// Возвращает минимальное значение из текущей суммы и указанного параметра.
    /// </summary>
    /// <param name="b">Сумма для сравнения.</param>
    /// <returns>Минимальное из двух значений.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Money Min(in Money b) => Value < b.Value ? this : b;

    /// <summary>
    /// Возвращает максимальное значение из текущей суммы и указанного параметра.
    /// </summary>
    /// <param name="b">Сумма для сравнения.</param>
    /// <returns>Максимальное из двух значений.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Money Max(in Money b) => Value > b.Value ? this : b;

    /// <summary>
    /// Возвращает строковое представление суммы в формате с двумя десятичными знаками.
    /// </summary>
    /// <returns>Строковое представление суммы.</returns>
    public override string ToString() => (Value / 100.0).ToString("N2", CultureInfo.InvariantCulture);

    /// <summary>
    /// Форматирует значение в строковом представлении с использованием указанного формата и провайдера форматирования.
    /// </summary>
    /// <param name="format">Строка формата (поддерживаются "G", "F", "N" или null для формата по умолчанию).</param>
    /// <param name="formatProvider">Объект, который предоставляет информацию о форматировании.</param>
    /// <returns>Строка, представляющая форматированное значение Money.</returns>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        formatProvider ??= CultureInfo.InvariantCulture;
        format ??= "N2";

        return (Value / 100.0).ToString(format, formatProvider);
    }

    /// <summary>
    /// Пытается форматировать значение в буфере символов, используя указанный формат и провайдер форматирования.
    /// </summary>
    /// <param name="destination">Буфер символов для форматирования результата.</param>
    /// <param name="charsWritten">Количество записанных символов в буфер.</param>
    /// <param name="format">Строка формата</param>
    /// <param name="formatProvider">Объект, который предоставляет информацию о форматировании.</param>
    /// <returns>True, если форматирование выполнено успешно; в противном случае — false.</returns>
    public bool TryFormat(
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? formatProvider)
    {
        formatProvider ??= CultureInfo.InvariantCulture;
        var formatString = format.IsEmpty ? "N2" : format;
        var decimalValue = Value / 100.0M;

        // Используем стандартный метод форматирования double для вывода в буфер
        return decimalValue.TryFormat(destination, out charsWritten, formatString, formatProvider);
    }

    /// <summary>
    /// Пытается форматировать значение в виде последовательности байтов UTF-8 в предоставленном буфере.
    /// </summary>
    /// <param name="utf8Destination">Буфер байтов UTF-8 для вывода форматированного значения.</param>
    /// <param name="bytesWritten">Количество байтов, записанных в буфер.</param>
    /// <param name="format">Строка формата.</param>
    /// <param name="formatProvider">Объект, предоставляющий информацию о форматировании.</param>
    /// <returns>True, если форматирование выполнено успешно; в противном случае — false.</returns>
    public bool TryFormat(
        Span<byte> utf8Destination,
        out int bytesWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? formatProvider)
    {
        formatProvider ??= CultureInfo.InvariantCulture;
        var formatString = format.IsEmpty ? "N2" : format.ToString();
        var decimalValue = Value / 100.0M;

        return decimalValue.TryFormat(utf8Destination, out bytesWritten, formatString, formatProvider);
    }

    [DoesNotReturn]
    private static void ThrowOverflow() => throw new OverflowException("Fraction should be between 0 and 100");

    #region + and -

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Money operator +(in Money left, in Money right) => new(left.Value + right.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Money operator -(in Money left, in Money right) => new(left.Value - right.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Money operator -(in Money money) => new(-money.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Money operator +(in Money money) => money;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Money operator +(in Money left, in long right) => new(left.Value + right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Money operator -(in Money left, in long right) => new(left.Value - right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Money operator +(in long left, in Money right) => new(left + right.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Money operator -(in long left, in Money right) => new(left - right.Value);

    #endregion

    #region * and /

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Money operator *(in Money left, in Money right) => new(left.Value * right.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Money operator /(in Money left, in Money right) => new(left.Value / right.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Money operator *(in long left, in Money right) => new(left * right.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Money operator /(in long left, in Money right) => new(left / right.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Money operator *(in Money left, in long right) => new(left.Value * right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Money operator /(in Money left, in long right) => new(left.Value / right);

    #endregion

    #region Equality

    public int CompareTo(Money other) => Value.CompareTo(other.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Money other) => Value == other.Value;

    public override bool Equals(object? obj) => obj is Money other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in Money left, in Money right) => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in Money left, in Money right) => !left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(in Money left, in Money right) => left.Value > right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(in Money left, in Money right) => left.Value >= right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(in Money left, in Money right) => left.Value < right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(in Money left, in Money right) => left.Value <= right.Value;

    #endregion

    #region implicit

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator float(in Money money) => money.Value / 100f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator double(in Money money) => money.Value / 100.0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator decimal(in Money money) => money.Value / 100m;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Money(float value) => new((long)(value * 100));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Money(double value) => new((long)(value * 100));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Money(decimal value) => new((long)(value * 100));

    #endregion

    #region interfaces

    static Money IMinMaxValue<Money>.MaxValue => MaxValue;
    static Money IMinMaxValue<Money>.MinValue => MinValue;

    #endregion
}