using System.Collections.Concurrent;
using System.Text;
using Hexecs.Loggers;
using Hexecs.Loggers.Sinks;
using Hexecs.Loggers.Writers;

namespace Hexecs.Tests.Loggers;

public sealed class TextSinkShould
{
    [Theory(DisplayName = "Проверять доступность уровня логирования корректно")]
    [InlineData(LogLevel.Trace, LogLevel.Trace, true)]
    [InlineData(LogLevel.Debug, LogLevel.Trace, false)]
    [InlineData(LogLevel.Info, LogLevel.Debug, false)]
    [InlineData(LogLevel.Warning, LogLevel.Info, false)]
    [InlineData(LogLevel.Error, LogLevel.Warning, false)]
    [InlineData(LogLevel.Error, LogLevel.Error, true)]
    public void CheckEnabledLevelCorrectly(LogLevel minLevel, LogLevel checkLevel, bool expected)
    {
        // Arrange
        var mockWriter = new Mock<StreamWriter>(new MemoryStream(), Encoding.UTF8, 1024, true);
        var sink = new TextSink(
            minLevel,
            [],
            new ConcurrentDictionary<Type, ILogValueWriter>(),
            mockWriter.Object
        );

        // Act
        var result = sink.IsEnabled(checkLevel);

        // Assert
        result.Should().Be(expected);
    }

    [Theory(DisplayName = "Записывать сообщение без параметров в поток")]
    [AutoData]
    public void WriteMessageWithoutParameters(string context, string message)
    {
        // Arrange
        var memoryStream = new MemoryStream();
        var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8, 1024, true);

        var sink = new TextSink(
            LogLevel.Trace,
            [],
            new ConcurrentDictionary<Type, ILogValueWriter>(),
            streamWriter
        );

        // Act
        sink.Write(LogLevel.Info, context, message);
        streamWriter.Flush();

        // Assert
        var result = Encoding.UTF8.GetString(memoryStream.ToArray());
        result.Should().Contain("[INF ");
        result.Should().Contain(context);
        result.Should().Contain(message);
    }

    [Fact(DisplayName = "Использовать DefaultValueWriter если фабрика не может создать WriterValue")]
    public void UseDefaultValueWriterIfFactoryCannotCreateWriter()
    {
        // Arrange
        var memoryStream = new MemoryStream();
        var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8, 1024, true);

        var valueWriterFactory = new Mock<ILogValueWriterFactory>();
        ILogValueWriter<int>? nullWriter = null;

        valueWriterFactory.Setup(x => x.TryCreateWriter<int>(out nullWriter)).Returns(false);

        var valueWriters = new ConcurrentDictionary<Type, ILogValueWriter>();

        var sink = new TextSink(
            LogLevel.Trace,
            [valueWriterFactory.Object],
            valueWriters,
            streamWriter
        );

        // Act
        sink.Write(LogLevel.Info, "TestContext", "Value: {1}", 42);
        streamWriter.Flush();

        // Assert
        valueWriterFactory.Verify(x => x.TryCreateWriter<int>(out nullWriter), Times.Once);
        valueWriters.Should().ContainKey(typeof(int));
        valueWriters[typeof(int)].Should().BeOfType(typeof(DefaultValueWriter<int>));
    }

    [Fact(DisplayName = "Вызывать FlushAsync и DisposeAsync для StreamWriter при вызове DisposeAsync")]
    public async Task CallFlushAsyncAndDisposeAsyncForStreamWriterWhenDisposingAsync()
    {
        // Arrange
        var mockWriter = new Mock<StreamWriter>(new MemoryStream(), Encoding.UTF8, 1024, true);

        mockWriter.Setup(x => x.FlushAsync()).Returns(Task.CompletedTask);
        mockWriter.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var sink = new TextSink(
            LogLevel.Trace,
            [],
            new ConcurrentDictionary<Type, ILogValueWriter>(),
            mockWriter.Object
        );

        // Act
        await sink.DisposeAsync();

        // Assert
        mockWriter.Verify(x => x.FlushAsync(), Times.Once);
        mockWriter.Verify(x => x.DisposeAsync(), Times.Once);
    }

    [Fact(DisplayName = "Корректно добавлять префикс с уровнем лога, датой и контекстом")]
    public void AddPrefixWithLevelDateAndContext()
    {
        // Arrange
        var memoryStream = new MemoryStream();
        var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8, 1024, true);

        var sink = new TextSink(
            LogLevel.Trace,
            [],
            new ConcurrentDictionary<Type, ILogValueWriter>(),
            streamWriter
        );

        // Act
        sink.Write(LogLevel.Info, "TestContext", "Test message");
        streamWriter.Flush();

        // Assert
        var result = Encoding.UTF8.GetString(memoryStream.ToArray());
        result.Should().MatchRegex(@"\[INF \d{2}:\d{2}:\d{2}\] TestContext: Test message");
    }

    [Fact(DisplayName = "Использовать корректные префиксы для различных уровней логирования")]
    public void UseCorrectPrefixesForDifferentLogLevels()
    {
        // Arrange
        var traceStream = new MemoryStream();
        var traceWriter = new StreamWriter(traceStream, Encoding.UTF8, 1024, true);
        var traceSink = new TextSink(
            LogLevel.Trace,
            [],
            new ConcurrentDictionary<Type, ILogValueWriter>(),
            traceWriter
        );

        var debugStream = new MemoryStream();
        var debugWriter = new StreamWriter(debugStream, Encoding.UTF8, 1024, true);
        var debugSink = new TextSink(
            LogLevel.Trace,
            [],
            new ConcurrentDictionary<Type, ILogValueWriter>(),
            debugWriter
        );

        var infoStream = new MemoryStream();
        var infoWriter = new StreamWriter(infoStream, Encoding.UTF8, 1024, true);
        var infoSink = new TextSink(
            LogLevel.Trace,
            [],
            new ConcurrentDictionary<Type, ILogValueWriter>(),
            infoWriter
        );

        var warnStream = new MemoryStream();
        var warnWriter = new StreamWriter(warnStream, Encoding.UTF8, 1024, true);
        var warnSink = new TextSink(
            LogLevel.Trace,
            [],
            new ConcurrentDictionary<Type, ILogValueWriter>(),
            warnWriter
        );

        var errorStream = new MemoryStream();
        var errorWriter = new StreamWriter(errorStream, Encoding.UTF8, 1024, true);
        var errorSink = new TextSink(
            LogLevel.Trace,
            [],
            new ConcurrentDictionary<Type, ILogValueWriter>(),
            errorWriter
        );

        // Act
        traceSink.Write(LogLevel.Trace, "Context", "Message");
        debugSink.Write(LogLevel.Debug, "Context", "Message");
        infoSink.Write(LogLevel.Info, "Context", "Message");
        warnSink.Write(LogLevel.Warning, "Context", "Message");
        errorSink.Write(LogLevel.Error, "Context", "Message");

        traceWriter.Flush();
        debugWriter.Flush();
        infoWriter.Flush();
        warnWriter.Flush();
        errorWriter.Flush();

        // Assert
        Encoding.UTF8.GetString(traceStream.ToArray()).Should().Contain("[TRC ");
        Encoding.UTF8.GetString(debugStream.ToArray()).Should().Contain("[DBG ");
        Encoding.UTF8.GetString(infoStream.ToArray()).Should().Contain("[INF ");
        Encoding.UTF8.GetString(warnStream.ToArray()).Should().Contain("[WRN ");
        Encoding.UTF8.GetString(errorStream.ToArray()).Should().Contain("[ERR ");
    }
}