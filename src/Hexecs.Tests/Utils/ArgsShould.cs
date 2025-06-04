using Hexecs.Utils;

namespace Hexecs.Tests.Utils;

public sealed class ArgsShould
{
    [Fact(DisplayName = "Метод Rent() должен возвращать экземпляр Args")]
    public void Rent_ShouldReturnArgsInstance()
    {
        // Arrange

        // Act
        var args = Args.Rent();

        // Assert
        args.Should().NotBeNull();
        args.Should().BeOfType<Args>();
    }

    [Theory(DisplayName = "Метод Rent<TValue>(name, value) должен возвращать экземпляр Args с установленным значением")]
    [AutoData]
    public void Rent_WithNameAndValue_ShouldReturnArgsInstanceWithSetValue(string name, int value)
    {
        // Arrange

        // Act
        var args = Args.Rent(name, value);

        // Assert
        args.Should().NotBeNull();
        args.Get<int>(name).Should().Be(value);
    }

    [Theory(DisplayName = "Метод Get<TValue>(name) должен возвращать установленное значение")]
    [AutoData]
    public void Get_ShouldReturnSetValue(string name, string value)
    {
        // Arrange
        var args = Args.Rent().Set(name, value);

        // Act
        var result = args.Get<string>(name);

        // Assert
        result.Should().Be(value);
    }

    [Theory(DisplayName = "Метод Get<TValue>(name) должен выбрасывать исключение, если значение не найдено")]
    [AutoData]
    public void Get_ShouldThrowException_WhenValueNotFound(string name)
    {
        // Arrange
        var args = Args.Rent();

        // Act
        Action act = () => args.Get<int>(name);

        // Assert
        act
            .Should()
            .Throw<Exception>()
            .WithMessage($"*Value '{name}'*");
    }

    [Fact(DisplayName = "Метод Return() должен очищать все хранилища значений")]
    public void Return_ShouldClearAllValueStorages()
    {
        // Arrange
        var args = Args.Rent()
            .Set("intValue", 42)
            .Set("stringValue", "test")
            .Set("boolValue", true);

        // Act
        args.Return();
        args = Args.Rent(); // Получаем тот же экземпляр из пула

        // Assert
        args.TryGet<int>("intValue", out _).Should().BeFalse();
        args.TryGet<string>("stringValue", out _).Should().BeFalse();
        args.TryGet<bool>("boolValue", out _).Should().BeFalse();
    }

    [Theory(DisplayName =
        "Метод TryGet<TValue>(name, out value) должен возвращать true и значение, если значение найдено")]
    [AutoData]
    public void TryGet_ShouldReturnTrueAndValue_WhenValueExists(string name, double value)
    {
        // Arrange
        var args = Args.Rent().Set(name, value);

        // Act
        var result = args.TryGet<double>(name, out var outValue);

        // Assert
        result.Should().BeTrue();
        outValue.Should().Be(value);
    }

    [Theory(DisplayName = "Метод TryGet<TValue>(name, out value) должен возвращать false, если значение не найдено")]
    [AutoData]
    public void TryGet_ShouldReturnFalse_WhenValueNotFound(string name)
    {
        // Arrange
        var args = Args.Rent();

        // Act
        var result = args.TryGet<DateTime>(name, out var outValue);

        // Assert
        result.Should().BeFalse();
        outValue.Should().Be(default);
    }

    [Theory(DisplayName = "Метод TryGet<TValue>(name, out value) должен возвращать false, если тип не совпадает")]
    [AutoData]
    public void TryGet_ShouldReturnFalse_WhenTypeDoesNotMatch(string name, int value)
    {
        // Arrange
        var args = Args.Rent().Set(name, value);

        // Act
        var result = args.TryGet<string>(name, out var outValue);

        // Assert
        result.Should().BeFalse();
        outValue.Should().Be(null);
    }

    [Theory(DisplayName = "Метод Set<TValue>(name, value) должен добавлять новое значение")]
    [AutoData]
    public void Set_ShouldAddNewValue(string name, Guid value)
    {
        // Arrange
        var args = Args.Rent();

        // Act
        var result = args.Set(name, value);

        // Assert
        result.Should().BeSameAs(args);
        args.TryGet<Guid>(name, out var outValue).Should().BeTrue();
        outValue.Should().Be(value);
    }

    [Theory(DisplayName = "Метод Set<TValue>(name, value) должен обновлять существующее значение")]
    [AutoData]
    public void Set_ShouldUpdateExistingValue(string name, int initialValue, int newValue)
    {
        // Arrange
        var args = Args.Rent().Set(name, initialValue);

        // Act
        var result = args.Set(name, newValue);

        // Assert
        result.Should().BeSameAs(args);
        args.Get<int>(name).Should().Be(newValue);
    }

    [Fact(DisplayName = "Метод GetEnumerator() должен перечислять все добавленные значения")]
    public void GetEnumerator_ShouldEnumerateAllAddedValues()
    {
        // Arrange
        var args = Args.Rent()
            .Set("intValue", 42)
            .Set("stringValue", "test")
            .Set("boolValue", true);

        var expected = new Dictionary<string, object>
        {
            { "intValue", 42 },
            { "stringValue", "test" },
            { "boolValue", true }
        };

        // Act
        var result = args.ToDictionary(kv => kv.Key, kv => kv.Value);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Theory(DisplayName =
        "Метод Rent должен возвращать существующий экземпляр из пула, если он был возвращен методом Return")]
    [AutoData]
    public void Rent_ShouldReturnExistingInstanceFromPool(string name, int value)
    {
        // Arrange
        var args1 = Args.Rent().Set(name, value);
        args1.Return(); // Возвращаем в пул

        // Act
        var args2 = Args.Rent();

        // Assert
        args2.Should().NotBeNull();
        args2.TryGet<int>(name, out _).Should().BeFalse(); // Значения должны быть очищены
    }

    [Fact(DisplayName = "Класс Args должен поддерживать разные типы значений")]
    public void Args_ShouldSupportDifferentValueTypes()
    {
        // Arrange - подготовка тестовых данных
        const int intValue = 42;
        const double doubleValue = 3.14;
        const string stringValue = "test";
        const bool boolValue = true;
        var dateTimeValue = new DateTime(2023, 1, 1);
        var guidValue = Guid.NewGuid();
        var customValue = new CustomValue { Value = "custom" };

        // Act - заполнение объекта Args всеми тестовыми значениями
        var args = Args
            .Rent()
            .Set("intValue", intValue)
            .Set("doubleValue", doubleValue)
            .Set("doubleValue", doubleValue)
            .Set("stringValue", stringValue)
            .Set("boolValue", boolValue)
            .Set("dateTimeValue", dateTimeValue)
            .Set("guidValue", guidValue)
            .Set("customValue", customValue);

        // Assert - проверка корректности получения всех типов данных
        args.Get<int>("intValue").Should().Be(intValue);
        args.Get<double>("doubleValue").Should().Be(doubleValue);
        args.Get<string>("stringValue").Should().Be(stringValue);
        args.Get<bool>("boolValue").Should().Be(boolValue);
        args.Get<DateTime>("dateTimeValue").Should().Be(dateTimeValue);
        args.Get<Guid>("guidValue").Should().Be(guidValue);
        args.Get<CustomValue>("customValue").Value.Should().Be("custom");
    }

    [Fact(DisplayName = "После вызова Return все значения типа должны быть недоступны")]
    public void AfterReturn_AllValuesOfTypeShouldBeUnavailable()
    {
        // Arrange
        var args = Args.Rent()
            .Set("value1", 1)
            .Set("value2", 2)
            .Set("value3", 3);

        // Act
        args.Return();
        args = Args.Rent(); // Получаем тот же экземпляр из пула

        // Assert
        args.TryGet<int>("value1", out _).Should().BeFalse();
        args.TryGet<int>("value2", out _).Should().BeFalse();
        args.TryGet<int>("value3", out _).Should().BeFalse();
    }

    private class CustomValue
    {
        public required string Value { get; set; }
    }
}