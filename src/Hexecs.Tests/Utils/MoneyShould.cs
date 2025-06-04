using Hexecs.Utils;

namespace Hexecs.Tests.Utils;

public class MoneyTests
{
    [Fact(DisplayName = "Конструктор создает экземпляр с правильным значением")]
    public void Constructor_ShouldCreateInstanceWithCorrectValue()
    {
        // Arrange & Act
        var money = new Money(10050);

        // Assert
        money.Value.Should().Be(10050);
        money.Whole.Should().Be(100);
        money.Fraction.Should().Be(50);
    }

    [Fact(DisplayName = "Zero должен возвращать экземпляр с нулевым значением")]
    public void Zero_ShouldReturnInstanceWithZeroValue()
    {
        // Act
        var zero = Money.Zero;

        // Assert
        zero.Value.Should().Be(0);
        zero.Whole.Should().Be(0);
        zero.Fraction.Should().Be(0);
    }

    [Theory(DisplayName = "Create должен корректно создавать экземпляр с заданными значениями")]
    [InlineData(100, 50, 10050)]
    [InlineData(0, 0, 0)]
    [InlineData(-100, 50, -10050)]
    public void Create_ShouldCreateCorrectInstance(long whole, int fraction, long expectedValue)
    {
        // Act
        var money = Money.Create(whole, fraction);

        // Assert
        money.Value.Should().Be(expectedValue);
        money.Whole.Should().Be(whole);
        money.Fraction.Should().Be(fraction);
    }

    [Fact(DisplayName = "Create должен вызывать исключение, если дробная часть вне диапазона")]
    public void Create_ShouldThrowOverflowException_WhenFractionOutOfRange()
    {
        // Act & Assert
        Action act1 = () => Money.Create(100, -1);
        act1.Should().Throw<OverflowException>()
            .WithMessage("Fraction should be between 0 and 100");

        Action act2 = () => Money.Create(100, 100);
        act2.Should().Throw<OverflowException>()
            .WithMessage("Fraction should be between 0 and 100");
    }

    [Theory(DisplayName = "TryParse должен корректно преобразовывать строку в Money")]
    [InlineData("100.50", 10050, true)]
    [InlineData("0", 0, true)]
    [InlineData("-100.50", -10050, true)]
    [InlineData("invalid", 0, false)]
    public void TryParse_ShouldCorrectlyParseString(string input, long expectedValue, bool expectedResult)
    {
        // Act
        var success = Money.TryParse(input, out var result);

        // Assert
        success.Should().Be(expectedResult);
        if (expectedResult)
        {
            result.Value.Should().Be(expectedValue);
        }
    }

    [Fact(DisplayName = "Abs должен возвращать абсолютное значение")]
    public void Abs_ShouldReturnAbsoluteValue()
    {
        // Arrange
        var negative = new Money(-10050);
        var positive = new Money(10050);

        // Act
        var absNegative = negative.Abs();
        var absPositive = positive.Abs();

        // Assert
        absNegative.Value.Should().Be(10050);
        absPositive.Value.Should().Be(10050);
    }

    [Fact(DisplayName = "Min должен возвращать минимальное значение")]
    public void Min_ShouldReturnMinimumValue()
    {
        // Arrange
        var money1 = new Money(10050);
        var money2 = new Money(20050);

        // Act
        var min1 = money1.Min(money2);
        var min2 = money2.Min(money1);

        // Assert
        min1.Value.Should().Be(10050);
        min2.Value.Should().Be(10050);
    }

    [Fact(DisplayName = "Max должен возвращать максимальное значение")]
    public void Max_ShouldReturnMaximumValue()
    {
        // Arrange
        var money1 = new Money(10050);
        var money2 = new Money(20050);

        // Act
        var max1 = money1.Max(money2);
        var max2 = money2.Max(money1);

        // Assert
        max1.Value.Should().Be(20050);
        max2.Value.Should().Be(20050);
    }

    [Fact(DisplayName = "ToString должен возвращать корректное строковое представление")]
    public void ToString_ShouldReturnCorrectStringRepresentation()
    {
        // Arrange
        var money = new Money(10050);
        var negMoney = new Money(-10050);

        // Act
        var str = money.ToString();
        var negStr = negMoney.ToString();

        // Assert
        str.Should().Be("100.50");
        negStr.Should().Be("-100.50");
    }

    [Theory(DisplayName = "ToString с форматом должен корректно форматировать значение")]
    [InlineData(null, null, "100.50")]
    [InlineData("F4", null, "100.5000")]
    [InlineData("N1", null, "100.5")]
    public void ToString_WithFormat_ShouldFormatValueCorrectly(string? format, IFormatProvider? provider, string expected)
    {
        // Arrange
        var money = new Money(10050);

        // Act
        var result = money.ToString(format, provider);

        // Assert
        result.Should().Be(expected);
    }

    [Fact(DisplayName = "TryFormat должен корректно форматировать значение в буфер символов")]
    public void TryFormat_ShouldFormatValueToCharSpan()
    {
        // Arrange
        var money = new Money(10050);
        var destination = new char[10];

        // Act
        var success = money.TryFormat(destination, out var charsWritten, "N2", null);

        // Assert
        success.Should().BeTrue();
        charsWritten.Should().BeGreaterThan(0);
        new string(destination, 0, charsWritten).Should().Be("100.50");
    }

    [Fact(DisplayName = "Операторы сложения должны работать корректно")]
    public void AdditionOperators_ShouldWorkCorrectly()
    {
        // Arrange
        var money1 = new Money(10050);
        var money2 = new Money(20050);
        var longValue = 100L;

        // Act & Assert
        (money1 + money2).Value.Should().Be(30100);
        (money1 + longValue).Value.Should().Be(10150);
        (longValue + money1).Value.Should().Be(10150);
        (+money1).Value.Should().Be(10050);
    }

    [Fact(DisplayName = "Операторы вычитания должны работать корректно")]
    public void SubtractionOperators_ShouldWorkCorrectly()
    {
        // Arrange
        var money1 = new Money(10050);
        var money2 = new Money(5050);
        var longValue = 100L;

        // Act & Assert
        (money1 - money2).Value.Should().Be(5000);
        (money1 - longValue).Value.Should().Be(9950);
        (longValue - money1).Value.Should().Be(-9950);
        (-money1).Value.Should().Be(-10050);
    }

    [Fact(DisplayName = "Операторы умножения должны работать корректно")]
    public void MultiplicationOperators_ShouldWorkCorrectly()
    {
        // Arrange
        var money1 = new Money(10050);
        var money2 = new Money(2);
        var longValue = 2L;

        // Act & Assert
        (money1 * money2).Value.Should().Be(20100);
        (money1 * longValue).Value.Should().Be(20100);
        (longValue * money1).Value.Should().Be(20100);
    }

    [Fact(DisplayName = "Операторы деления должны работать корректно")]
    public void DivisionOperators_ShouldWorkCorrectly()
    {
        // Arrange
        var money1 = new Money(10050);
        var money2 = new Money(2);
        var longValue = 2L;

        // Act & Assert
        (money1 / money2).Value.Should().Be(5025);
        (money1 / longValue).Value.Should().Be(5025);
        (longValue / money1).Value.Should().Be(0);
    }

    [Fact(DisplayName = "Операторы сравнения должны работать корректно")]
    public void ComparisonOperators_ShouldWorkCorrectly()
    {
        // Arrange
        var money1 = new Money(10050);
        var money2 = new Money(20050);
        var money3 = new Money(10050);

        // Act & Assert
        (money1 == money3).Should().BeTrue();
        (money1 != money2).Should().BeTrue();
        (money1 < money2).Should().BeTrue();
        (money2 > money1).Should().BeTrue();
        (money1 <= money3).Should().BeTrue();
        (money1 >= money3).Should().BeTrue();
        (money1 <= money2).Should().BeTrue();
        (money2 >= money1).Should().BeTrue();
    }

    [Fact(DisplayName = "Equals должен корректно определять равенство")]
    public void Equals_ShouldWorkCorrectly()
    {
        // Arrange
        var money1 = new Money(10050);
        var money2 = new Money(10050);
        var money3 = new Money(20050);
        var obj = new object();

        // Act & Assert
        money1.Equals(money2).Should().BeTrue();
        money1.Equals(money3).Should().BeFalse();
        money1.Equals(obj).Should().BeFalse();
    }

    [Fact(DisplayName = "CompareTo должен корректно сравнивать значения")]
    public void CompareTo_ShouldCompareValuesCorrectly()
    {
        // Arrange
        var money1 = new Money(10050);
        var money2 = new Money(20050);
        var money3 = new Money(10050);

        // Act & Assert
        money1.CompareTo(money2).Should().BeLessThan(0);
        money2.CompareTo(money1).Should().BeGreaterThan(0);
        money1.CompareTo(money3).Should().Be(0);
    }

    [Fact(DisplayName = "GetHashCode должен возвращать корректный хэш-код")]
    public void GetHashCode_ShouldReturnCorrectHashCode()
    {
        // Arrange
        var money = new Money(10050);

        // Act
        var hashCode = money.GetHashCode();

        // Assert
        hashCode.Should().Be(10050.GetHashCode());
    }

    [Fact(DisplayName = "Неявное преобразование в числовые типы должно работать корректно")]
    public void ImplicitConversionToNumericTypes_ShouldWorkCorrectly()
    {
        // Arrange
        var money = new Money(10050);

        // Act
        float floatValue = money;
        double doubleValue = money;
        decimal decimalValue = money;

        // Assert
        floatValue.Should().BeApproximately(100.5f, 0.001f);
        doubleValue.Should().BeApproximately(100.5, 0.001);
        decimalValue.Should().Be(100.5m);
    }

    [Fact(DisplayName = "Неявное преобразование из числовых типов должно работать корректно")]
    public void ImplicitConversionFromNumericTypes_ShouldWorkCorrectly()
    {
        // Act
        Money moneyFromFloat = 100.5f;
        Money moneyFromDouble = 100.5;
        Money moneyFromDecimal = 100.5m;

        // Assert
        moneyFromFloat.Value.Should().Be(10050);
        moneyFromDouble.Value.Should().Be(10050);
        moneyFromDecimal.Value.Should().Be(10050);
    }

    [Fact(DisplayName = "MaxValue и MinValue должны содержать правильные значения")]
    public void MaxValueAndMinValue_ShouldHaveCorrectValues()
    {
        // Act & Assert
        Money.MaxValue.Should().Be(Money.Create(long.MaxValue / 100L - 1, 99));
        Money.MinValue.Should().Be(Money.Create(long.MinValue / 100L + 1, 99));
    }
}