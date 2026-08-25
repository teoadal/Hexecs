using Hexecs.Loggers;

namespace Hexecs.Tests.Loggers;

public sealed class LogServiceShould
{
    [Fact(DisplayName = "Создать экземпляр пустого логгера через свойство Empty")]
    public void CreateEmptyLoggerInstance()
    {
        // Arrange
            
        // Act
        var emptyLogger = LogService.Empty;
            
        // Assert
        emptyLogger.IsEnabled(LogLevel.Info).Should().BeFalse();
    }
        
    [Theory(DisplayName = "Корректно проверять доступность уровня логирования")]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Info)]
    [InlineData(LogLevel.Warning)]
    [InlineData(LogLevel.Error)]
    public void CheckLevelEnabledCorrectly(LogLevel level)
    {
        // Arrange
        var mockSink1 = new Mock<ILogSink>();
        var mockSink2 = new Mock<ILogSink>();
            
        mockSink1.Setup(x => x.IsEnabled(level)).Returns(true);
        mockSink2.Setup(x => x.IsEnabled(level)).Returns(false);
            
        var logService = new LogService(new[] { mockSink1.Object, mockSink2.Object });
            
        // Act
        var isEnabled = logService.IsEnabled(level);
            
        // Assert
        isEnabled.Should().BeTrue();
    }
        
    [Fact(DisplayName = "Вернуть false для IsEnabled когда нет писателей")]
    public void ReturnFalseForIsEnabledWhenNoWriters()
    {
        // Arrange
        var logService = new LogService(Array.Empty<ILogSink>());
            
        // Act
        var isEnabled = logService.IsEnabled(LogLevel.Info);
            
        // Assert
        isEnabled.Should().BeFalse();
    }
        
    [Fact(DisplayName = "Вернуть false для IsEnabled когда все писатели выключены")]
    public void ReturnFalseForIsEnabledWhenAllWritersDisabled()
    {
        // Arrange
        var mockSink1 = new Mock<ILogSink>();
        var mockSink2 = new Mock<ILogSink>();
            
        mockSink1.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(false);
        mockSink2.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(false);
            
        var logService = new LogService(new[] { mockSink1.Object, mockSink2.Object });
            
        // Act
        var isEnabled = logService.IsEnabled(LogLevel.Info);
            
        // Assert
        isEnabled.Should().BeFalse();
    }
        
    [Fact(DisplayName = "Создать контекстный логгер с строковым контекстом")]
    public void CreateContextLoggerWithStringContext()
    {
        // Arrange
        var mockSink = new Mock<ILogSink>();
        var logService = new LogService(new[] { mockSink.Object });
        var contextName = "TestContext";
            
        // Act
        var logger = logService.CreateContext(contextName);
            
        // Assert
        logger.Should().NotBeNull();
        logger.Should().BeOfType<ContextLogger>();
    }
        
    [Fact(DisplayName = "Создать контекстный логгер с контекстом на основе типа")]
    public void CreateContextLoggerWithTypeContext()
    {
        // Arrange
        var mockSink = new Mock<ILogSink>();
        var logService = new LogService(new[] { mockSink.Object });
            
        // Act
        var logger = logService.CreateContext(typeof(LogServiceShould));
            
        // Assert
        logger.Should().NotBeNull();
        logger.Should().BeOfType<ContextLogger>();
    }
        
    [Theory(DisplayName = "Записывать сообщение без параметров во все писатели")]
    [AutoData]
    public void WriteMessageToAllWriters(string context, string template, LogLevel level)
    {
        // Arrange
        var mockSink1 = new Mock<ILogSink>();
        var mockSink2 = new Mock<ILogSink>();
            
        var logService = new LogService(new[] { mockSink1.Object, mockSink2.Object });
            
        // Act
        logService.Write(level, context, template);
            
        // Assert
        mockSink1.Verify(x => x.Write(level, context, template), Times.Once);
        mockSink2.Verify(x => x.Write(level, context, template), Times.Once);
    }
        
    [Theory(DisplayName = "Записывать сообщение с одним параметром во все писатели")]
    [AutoData]
    public void WriteMessageWithOneParameterToAllWriters(string context, string template, int arg1, LogLevel level)
    {
        // Arrange
        var mockSink1 = new Mock<ILogSink>();
        var mockSink2 = new Mock<ILogSink>();
            
        var logService = new LogService(new[] { mockSink1.Object, mockSink2.Object });
            
        // Act
        logService.Write(level, context, template, arg1);
            
        // Assert
        mockSink1.Verify(x => x.Write(level, context, template, arg1), Times.Once);
        mockSink2.Verify(x => x.Write(level, context, template, arg1), Times.Once);
    }
        
    [Theory(DisplayName = "Записывать сообщение с двумя параметрами во все писатели")]
    [AutoData]
    public void WriteMessageWithTwoParametersToAllWriters(string context, string template, int arg1, string arg2, LogLevel level)
    {
        // Arrange
        var mockSink1 = new Mock<ILogSink>();
        var mockSink2 = new Mock<ILogSink>();
            
        var logService = new LogService(new[] { mockSink1.Object, mockSink2.Object });
            
        // Act
        logService.Write(level, context, template, arg1, arg2);
            
        // Assert
        mockSink1.Verify(x => x.Write(level, context, template, arg1, arg2), Times.Once);
        mockSink2.Verify(x => x.Write(level, context, template, arg1, arg2), Times.Once);
    }
        
    [Theory(DisplayName = "Записывать сообщение с тремя параметрами во все писатели")]
    [AutoData]
    public void WriteMessageWithThreeParametersToAllWriters(string context, string template, 
        int arg1, string arg2, bool arg3, LogLevel level)
    {
        // Arrange
        var mockSink1 = new Mock<ILogSink>();
        var mockSink2 = new Mock<ILogSink>();
            
        var logService = new LogService(new[] { mockSink1.Object, mockSink2.Object });
            
        // Act
        logService.Write(level, context, template, arg1, arg2, arg3);
            
        // Assert
        mockSink1.Verify(x => x.Write(level, context, template, arg1, arg2, arg3), Times.Once);
        mockSink2.Verify(x => x.Write(level, context, template, arg1, arg2, arg3), Times.Once);
    }
        
    [Theory(DisplayName = "Записывать сообщение с четырьмя параметрами во все писатели")]
    [AutoData]
    public void WriteMessageWithFourParametersToAllWriters(string context, string template, 
        int arg1, string arg2, bool arg3, double arg4, LogLevel level)
    {
        // Arrange
        var mockSink1 = new Mock<ILogSink>();
        var mockSink2 = new Mock<ILogSink>();
            
        var logService = new LogService(new[] { mockSink1.Object, mockSink2.Object });
            
        // Act
        logService.Write(level, context, template, arg1, arg2, arg3, arg4);
            
        // Assert
        mockSink1.Verify(x => x.Write(level, context, template, arg1, arg2, arg3, arg4), Times.Once);
        mockSink2.Verify(x => x.Write(level, context, template, arg1, arg2, arg3, arg4), Times.Once);
    }
        
    [Theory(DisplayName = "Записывать сообщение с пятью параметрами во все писатели")]
    [AutoData]
    public void WriteMessageWithFiveParametersToAllWriters(string context, string template, 
        int arg1, string arg2, bool arg3, double arg4, char arg5, LogLevel level)
    {
        // Arrange
        var mockSink1 = new Mock<ILogSink>();
        var mockSink2 = new Mock<ILogSink>();
            
        var logService = new LogService(new[] { mockSink1.Object, mockSink2.Object });
            
        // Act
        logService.Write(level, context, template, arg1, arg2, arg3, arg4, arg5);
            
        // Assert
        mockSink1.Verify(x => x.Write(level, context, template, arg1, arg2, arg3, arg4, arg5), Times.Once);
        mockSink2.Verify(x => x.Write(level, context, template, arg1, arg2, arg3, arg4, arg5), Times.Once);
    }
        
    [Fact(DisplayName = "Вызывать Dispose у всех писателей при вызове Dispose")]
    public void DisposeAllWritersWhenDisposing()
    {
        // Arrange
        var mockSink1 = new Mock<ILogSink>();
        var mockSink2 = new Mock<ILogSink>();
            
        var logService = new LogService(new[] { mockSink1.Object, mockSink2.Object });
            
        // Act
        logService.Dispose();
            
        // Assert
        mockSink1.Verify(x => x.Dispose(), Times.Once);
        mockSink2.Verify(x => x.Dispose(), Times.Once);
    }
}