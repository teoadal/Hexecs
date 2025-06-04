using System.Collections.Concurrent;
using Hexecs.Configurations;

namespace Hexecs.Tests.Configurations;

public sealed class ConfigurationServiceShould
{
    [Fact(DisplayName = "GetValue должен возвращать значение из источника")]
    public void GetValue_ShouldReturnValueFromSource()
    {
        // Arrange
        var mockSource = new Mock<IConfigurationSource>();
        string outValue = "test-value";
        mockSource.Setup(s => s.TryGetValue(It.Is<string>(k => k == "test-key"), out outValue!))
            .Returns(true);

        var sources = new[] { mockSource.Object };
        var values = new ConcurrentDictionary<string, object?>();
        var service = new ConfigurationService(sources, values);

        // Act
        var result = service.GetValue<string>("test-key");

        // Assert
        result.Should().Be("test-value");
    }

    [Fact(DisplayName = "GetValue должен кэшировать значение после первого обращения")]
    public void GetValue_ShouldCacheValueAfterFirstAccess()
    {
        // Arrange
        var mockSource = new Mock<IConfigurationSource>();
        string outValue = "test-value";
        mockSource.Setup(s => s.TryGetValue(It.Is<string>(k => k == "test-key"), out outValue!))
            .Returns(true);

        var sources = new[] { mockSource.Object };
        var values = new ConcurrentDictionary<string, object?>();
        var service = new ConfigurationService(sources, values);

        // Act
        var result1 = service.GetValue<string>("test-key");
        var result2 = service.GetValue<string>("test-key");

        // Assert
        result1.Should().Be("test-value");
        result2.Should().Be("test-value");
        mockSource.Verify(s => s.TryGetValue<string>(It.IsAny<string>(), out It.Ref<string>.IsAny!), Times.Once);
    }

    [Fact(DisplayName = "GetValue должен вернуть null, если значение не найдено")]
    public void GetValue_ShouldReturnNull_WhenValueNotFound()
    {
        // Arrange
        var mockSource = new Mock<IConfigurationSource>();
        string? outValue = null;
        mockSource.Setup(s => s.TryGetValue(It.IsAny<string>(), out outValue))
            .Returns(false);

        var sources = new[] { mockSource.Object };
        var values = new ConcurrentDictionary<string, object?>();
        var service = new ConfigurationService(sources, values);

        // Act
        var result = service.GetValue<string>("non-existent-key");

        // Assert
        result.Should().BeNull();
    }

    [Fact(DisplayName = "GetValue должен проверять все источники по порядку")]
    public void GetValue_ShouldCheckAllSourcesInOrder()
    {
        // Arrange
        var mockSource1 = new Mock<IConfigurationSource>();
        string? outValue1 = null;
        mockSource1
            .Setup(s => s.TryGetValue(It.Is<string>(k => k == "test-key"), out outValue1))
            .Returns(false);

        var mockSource2 = new Mock<IConfigurationSource>();
        string outValue2 = "test-value";
        mockSource2
            .Setup(s => s.TryGetValue(It.Is<string>(k => k == "test-key"), out outValue2!))
            .Returns(true);

        var sources = new[] { mockSource1.Object, mockSource2.Object };
        var values = new ConcurrentDictionary<string, object?>();
        var service = new ConfigurationService(sources, values);

        // Act
        var result = service.GetValue<string>("test-key");

        // Assert
        result.Should().Be("test-value");
        mockSource1.Verify(s => s.TryGetValue<string>("test-key", out It.Ref<string>.IsAny!), Times.Once);
        mockSource2.Verify(s => s.TryGetValue<string>("test-key", out It.Ref<string>.IsAny!), Times.Once);
    }

    [Fact(DisplayName = "GetRequiredValue должен вернуть значение, если оно найдено")]
    public void GetRequiredValue_ShouldReturnValue_WhenFound()
    {
        // Arrange
        var mockSource = new Mock<IConfigurationSource>();
        int outValue = 42;
        mockSource.Setup(s => s.TryGetValue(It.Is<string>(k => k == "test-key"), out outValue))
            .Returns(true);

        var sources = new[] { mockSource.Object };
        var values = new ConcurrentDictionary<string, object?>();
        var service = new ConfigurationService(sources, values);

        // Act
        var result = service.GetRequiredValue<int>("test-key");

        // Assert
        result.Should().Be(42);
    }

    [Fact(DisplayName = "GetRequiredValue должен выбросить исключение, если значение не найдено")]
    public void GetRequiredValue_ShouldThrowException_WhenValueNotFound()
    {
        // Arrange
        var mockSource = new Mock<IConfigurationSource>();
        int outValue = 0;
        mockSource.Setup(s => s.TryGetValue(It.IsAny<string>(), out outValue))
            .Returns(false);

        var sources = new[] { mockSource.Object };
        var values = new ConcurrentDictionary<string, object?>();
        var service = new ConfigurationService(sources, values);

        // Act & Assert
        var act = () => service.GetRequiredValue<int>("non-existent-key");
        act.Should().Throw<Exception>();
    }

    [Fact(DisplayName = "Empty должен создать экземпляр с пустыми источниками")]
    public void Empty_ShouldCreateInstanceWithEmptySources()
    {
        // Act
        var service = ConfigurationService.Empty;

        // Assert
        var result = service.GetValue<string>("any-key");
        result.Should().BeNull();
    }

    [Theory(DisplayName = "GetValue должен правильно обрабатывать различные типы")]
    [InlineData(42)]
    [InlineData("test-string")]
    [InlineData(true)]
    public void GetValue_ShouldHandleDifferentTypes<T>(T expectedValue)
    {
        // Arrange
        var mockSource = new Mock<IConfigurationSource>();
            
        // Используем CallBase и создаем тестовый источник
        var testSource = new TestConfigurationSource<T>(expectedValue);
            
        var sources = new IConfigurationSource[] { testSource };
        var values = new ConcurrentDictionary<string, object?>();
        var service = new ConfigurationService(sources, values);

        // Act
        var result = service.GetValue<T>("test-key");

        // Assert
        result.Should().BeEquivalentTo(expectedValue);
    }
}

// Вспомогательный тестовый класс для работы с разными типами
public class TestConfigurationSource<T> : IConfigurationSource
{
    private readonly T _value;

    public TestConfigurationSource(T value)
    {
        _value = value;
    }

    public void Load()
    {
    }

    public bool TryGetValue<TValue>(string key, out TValue value)
    {
        if (typeof(TValue) == typeof(T))
        {
            value = (TValue)(object)_value!;
            return true;
        }
            
        value = default!;
        return false;
    }
}