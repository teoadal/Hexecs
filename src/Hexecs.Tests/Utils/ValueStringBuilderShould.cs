using System.Globalization;
using Hexecs.Utils;

namespace Hexecs.Tests.Utils;

public class ValueStringBuilderShould
{
    [Fact(DisplayName = "Создание пустого билдера через конструктор с буфером")]
    public void EmptyConstructor_CreatesEmptyBuilder()
    {
        // Arrange
        Span<char> buffer = stackalloc char[16];
            
        // Act
        var builder = new ValueStringBuilder(buffer);
            
        // Assert
        builder.Length.Should().Be(0);
        builder.ToString().Should().BeEmpty();
    }
        
    [Fact(DisplayName = "Создание пустого билдера через конструктор с ёмкостью")]
    public void CapacityConstructor_CreatesEmptyBuilder()
    {
        // Arrange & Act
        using var builder = new ValueStringBuilder(16);
            
        // Assert
        builder.Length.Should().Be(0);
        builder.ToString().Should().BeEmpty();
    }
        
    [Fact(DisplayName = "Добавление символов добавляет их в билдер")]
    public void Append_Char_AppendsToBuilder()
    {
        // Arrange
        Span<char> buffer = stackalloc char[16];
        var builder = new ValueStringBuilder(buffer);
            
        // Act
        builder.Append('a');
        builder.Append('b');
        builder.Append('c');
            
        // Assert
        builder.Length.Should().Be(3);
        builder.ToString().Should().Be("abc");
    }
        
    [Fact(DisplayName = "Добавление строк корректно добавляется в билдер")]
    public void Append_String_AppendsToBuilder()
    {
        // Arrange
        Span<char> buffer = stackalloc char[16];
        var builder = new ValueStringBuilder(buffer);
            
        // Act
        builder.Append("Hello");
        builder.Append(" ");
        builder.Append("World");
            
        // Assert
        builder.Length.Should().Be(11);
        builder.ToString().Should().Be("Hello World");
    }
        
    [Theory(DisplayName = "Добавление пустой или null строки не изменяет билдер")]
    [InlineData("")]
    [InlineData(null)]
    public void Append_EmptyOrNullString_NoEffect(string? input)
    {
        // Arrange
        Span<char> buffer = stackalloc char[16];
        var builder = new ValueStringBuilder(buffer);
            
        // Act
        builder.Append(input);
            
        // Assert
        builder.Length.Should().Be(0);
        builder.ToString().Should().BeEmpty();
    }
        
    [Theory(DisplayName = "Добавление булевых значений добавляет 'True' или 'False'")]
    [InlineData(true, "True")]
    [InlineData(false, "False")]
    public void Append_Bool_AppendsCorrectString(bool value, string expected)
    {
        // Arrange
        Span<char> buffer = stackalloc char[16];
        var builder = new ValueStringBuilder(buffer);
            
        // Act
        builder.Append(value);
            
        // Assert
        builder.ToString().Should().Be(expected);
    }
        
    [Theory(DisplayName = "Добавление целых чисел преобразует их в строки")]
    [InlineData(42, "42")]
    [InlineData(-123, "-123")]
    [InlineData(0, "0")]
    [InlineData(int.MaxValue, "2147483647")]
    [InlineData(int.MinValue, "-2147483648")]
    public void Append_Int_AppendsCorrectString(int value, string expected)
    {
        // Arrange
        Span<char> buffer = stackalloc char[16];
        var builder = new ValueStringBuilder(buffer);
            
        // Act
        builder.Append(value);
            
        // Assert
        builder.ToString().Should().Be(expected);
    }
        
    [Theory(DisplayName = "Добавление целых чисел с форматом применяет указанный формат")]
    [InlineData(42, "X4", "002A")]
    [InlineData(255, "D8", "00000255")]
    [InlineData(-1, "X", "FFFFFFFF")]
    public void Append_Int_WithFormat_AppendsCorrectString(int value, string format, string expected)
    {
        // Arrange
        Span<char> buffer = stackalloc char[32];
        var builder = new ValueStringBuilder(buffer);
            
        // Act
        builder.Append(value, format);
            
        // Assert
        builder.ToString().Should().Be(expected);
    }
        
    [Theory(DisplayName = "Добавление чисел с плавающей точкой преобразует их в строки")]
    [InlineData(3.14159, "3.14159")]
    [InlineData(-0.5, "-0.5")]
    [InlineData(0.0, "0")]
    public void Append_Double_AppendsCorrectString(double value, string expected)
    {
        // Arrange
        Span<char> buffer = stackalloc char[32];
        var builder = new ValueStringBuilder(buffer);
            
        // Act
        builder.Append(value, culture: CultureInfo.InvariantCulture);
            
        // Assert
        builder.ToString().Should().Be(expected);
    }
        
    [Theory(DisplayName = "Добавление чисел с плавающей точкой с форматом применяет формат")]
    [InlineData(3.14159, "F2", "3.14")]
    [InlineData(1234.5678, "E2", "1.23E+003")]
    [InlineData(0.5, "P0", "50 %")]
    public void Append_Double_WithFormat_AppendsCorrectString(double value, string format, string expected)
    {
        // Arrange
        Span<char> buffer = stackalloc char[32];
        var builder = new ValueStringBuilder(buffer);
            
        // Act
        builder.Append(value, format, CultureInfo.InvariantCulture);
            
        // Assert
        builder.ToString().Should().Be(expected);
    }
        
    [Fact(DisplayName = "Добавление GUID преобразует его в строку со стандартным форматом")]
    public void Append_Guid_AppendsCorrectString()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var builder = new ValueStringBuilder(buffer);
        var guid = new Guid("12345678-1234-1234-1234-123456789abc");
            
        // Act
        builder.Append(guid);
            
        // Assert
        builder.ToString().Should().Be("12345678-1234-1234-1234-123456789abc");
    }
        
    [Theory(DisplayName = "Добавление GUID с форматом применяет указанный формат")]
    [InlineData("D", "12345678-1234-1234-1234-123456789abc")]
    [InlineData("B", "{12345678-1234-1234-1234-123456789abc}")]
    [InlineData("P", "(12345678-1234-1234-1234-123456789abc)")]
    public void Append_Guid_WithFormat_AppendsCorrectString(string format, string expected)
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var builder = new ValueStringBuilder(buffer);
        var guid = new Guid("12345678-1234-1234-1234-123456789abc");
            
        // Act
        builder.Append(guid, format);
            
        // Assert
        builder.ToString().Should().Be(expected);
    }
        
    [Fact(DisplayName = "Добавление DateTime с форматом применяет указанный формат")]
    public void Append_DateTime_WithFormat_AppendsCorrectString()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var builder = new ValueStringBuilder(buffer);
        var date = new DateTime(2023, 10, 15, 14, 30, 45);
            
        // Act
        builder.Append(date, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            
        // Assert
        builder.ToString().Should().Be("2023-10-15 14:30:45");
    }
        
    [Theory(DisplayName = "Добавление TimeSpan с разными форматами применяет форматы")]
    [InlineData("c", "01:02:03")]
    [InlineData("g", "1:02:03")]
    public void Append_TimeSpan_WithFormat_AppendsCorrectString(string format, string expected)
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var builder = new ValueStringBuilder(buffer);
        var timeSpan = new TimeSpan(0, 1, 2, 3);
            
        // Act
        builder.Append(timeSpan, format, CultureInfo.InvariantCulture);
            
        // Assert
        builder.ToString().Should().Be(expected);
    }
        
    [Fact(DisplayName = "Добавление Span<char> добавляет все символы в билдер")]
    public void Append_CharSpan_AppendsCorrectString()
    {
        // Arrange
        Span<char> buffer = stackalloc char[32];
        var builder = new ValueStringBuilder(buffer);
        Span<char> value = stackalloc char[] { 'T', 'e', 's', 't' };
            
        // Act
        builder.Append(value);
            
        // Assert
        builder.ToString().Should().Be("Test");
    }
        
    [Fact(DisplayName = "Добавление ReadOnlySpan<char> добавляет все символы в билдер")]
    public void Append_ReadOnlyCharSpan_AppendsCorrectString()
    {
        // Arrange
        Span<char> buffer = stackalloc char[32];
        var builder = new ValueStringBuilder(buffer);
        ReadOnlySpan<char> value = "Hello";
            
        // Act
        builder.Append(value);
            
        // Assert
        builder.ToString().Should().Be("Hello");
    }
        
    [Fact(DisplayName = "Clear сбрасывает длину билдера без изменения буфера")]
    public void Clear_ResetsLength()
    {
        // Arrange
        Span<char> buffer = stackalloc char[32];
        var builder = new ValueStringBuilder(buffer);
        builder.Append("Test");
            
        // Act
        builder.Clear();
            
        // Assert
        builder.Length.Should().Be(0);
        builder.ToString().Should().BeEmpty();
            
        // Проверяем, что можно снова добавлять данные
        builder.Append("New");
        builder.ToString().Should().Be("New");
    }
        
    [Fact(DisplayName = "Whitespace добавляет пробел в билдер")]
    public void Whitespace_AddsSpace()
    {
        // Arrange
        Span<char> buffer = stackalloc char[32];
        var builder = new ValueStringBuilder(buffer);
        builder.Append("Hello");
            
        // Act
        builder.Whitespace();
        builder.Append("World");
            
        // Assert
        builder.ToString().Should().Be("Hello World");
    }
        
    [Theory(DisplayName = "TrimEnd удаляет различные пробельные символы в конце")]
    [InlineData("Hello  ", "Hello")]
    [InlineData("Hello\t\n ", "Hello")]
    [InlineData(" Hello ", " Hello")]
    [InlineData("Hello", "Hello")]
    public void TrimEnd_RemovesTrailingWhitespace(string input, string expected)
    {
        // Arrange
        Span<char> buffer = stackalloc char[32];
        var builder = new ValueStringBuilder(buffer);
        builder.Append(input);
            
        // Act
        builder.TrimEnd();
            
        // Assert
        builder.ToString().Should().Be(expected);
    }
        
    [Fact(DisplayName = "EnsureCapacity увеличивает буфер для требуемой ёмкости")]
    public void EnsureCapacity_GrowsBuffer()
    {
        // Arrange
        Span<char> buffer = stackalloc char[4];
        var builder = new ValueStringBuilder(buffer);
        builder.Append("AB");
            
        // Act
        builder.EnsureCapacity(10);
            
        // Проверяем, что существующее содержимое сохранилось
        builder.ToString().Should().Be("AB");
            
        // Проверяем, что можем добавить больше символов
        builder.Append("CDEFGHIJ");
            
        // Assert
        builder.ToString().Should().Be("ABCDEFGHIJ");
    }
        
    [Fact(DisplayName = "EnsureCapacity не изменяет буфер, если текущей ёмкости достаточно")]
    public void EnsureCapacity_DoesNotGrowBufferWhenCapacityIsSufficient()
    {
        // Arrange
        Span<char> buffer = stackalloc char[16];
        var builder = new ValueStringBuilder(buffer);
        builder.Append("AB");
            
        // Act
        builder.EnsureCapacity(5); // Меньше текущей ёмкости 16
            
        // Assert
        builder.ToString().Should().Be("AB");
    }
        
    [Fact(DisplayName = "ToReadonlySpan возвращает корректный ReadOnlySpan с содержимым")]
    public void ToReadonlySpan_ReturnsCorrectSpan()
    {
        // Arrange
        Span<char> buffer = stackalloc char[32];
        var builder = new ValueStringBuilder(buffer);
        builder.Append("Test");
            
        // Act
        ReadOnlySpan<char> span = builder.AsReadonlySpan();
            
        // Assert
        span.Length.Should().Be(4);
        span[0].Should().Be('T');
        span[1].Should().Be('e');
        span[2].Should().Be('s');
        span[3].Should().Be('t');
            
        // Проверка всего span целиком
        span.ToString().Should().Be("Test");
    }
        
    [Fact(DisplayName = "Автоматическое увеличение буфера при превышении ёмкости")]
    public void AutomaticBufferGrowth_WhenAppendingBeyondCapacity()
    {
        // Arrange
        Span<char> buffer = stackalloc char[4];
        var builder = new ValueStringBuilder(buffer);
            
        // Act
        builder.Append("1234"); // Заполняем исходный буфер
        builder.Append("5678"); // Должно вызвать увеличение буфера
            
        // Assert
        builder.ToString().Should().Be("12345678");
        builder.Length.Should().Be(8);
    }
        
    [Fact(DisplayName = "Добавление uint значений преобразует их в строки")]
    public void Append_UInt_AppendsCorrectString()
    {
        // Arrange
        Span<char> buffer = stackalloc char[32];
        var builder = new ValueStringBuilder(buffer);
            
        // Act
        builder.Append(uint.MaxValue);
            
        // Assert
        builder.ToString().Should().Be("4294967295");
    }
        
    [Fact(DisplayName = "Добавление long значений преобразует их в строки")]
    public void Append_Long_AppendsCorrectString()
    {
        // Arrange
        Span<char> buffer = stackalloc char[32];
        var builder = new ValueStringBuilder(buffer);
            
        // Act
        builder.Append(long.MaxValue);
            
        // Assert
        builder.ToString().Should().Be("9223372036854775807");
    }
        
    [Theory(DisplayName = "Добавление одиночного символа оптимизировано для строк длины 1")]
    [InlineData("A")]
    [InlineData("1")]
    [InlineData("!")]
    public void Append_SingleCharacterString_UsesOptimizedPath(string singleChar)
    {
        // Arrange
        Span<char> buffer = stackalloc char[16];
        var builder = new ValueStringBuilder(buffer);
            
        // Act
        builder.Append(singleChar);
            
        // Assert
        builder.Length.Should().Be(1);
        builder.ToString().Should().Be(singleChar);
    }
}