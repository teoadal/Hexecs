using System.Collections.Concurrent;
using System.Text;
using Hexecs.Actors;
using Hexecs.Actors.Loggers;
using Hexecs.Assets;
using Hexecs.Assets.Loggers;
using Hexecs.Loggers.Sinks;
using Hexecs.Loggers.Writers;

namespace Hexecs.Loggers;

public sealed class LogBuilder
{
    private readonly Queue<ILogValueWriterFactory> _valueFactories; // для замены уже существующих
    private readonly ConcurrentDictionary<Type, ILogValueWriter> _valueWriters;
    private readonly List<ILogSink> _writers = [];

    private LogLevel _level = LogLevel.Trace;

    internal LogBuilder()
    {
        _valueWriters = new ConcurrentDictionary<Type, ILogValueWriter>([
            new KeyValuePair<Type, ILogValueWriter>(typeof(DateTime), new DefaultDateTimeWriter()),
            new KeyValuePair<Type, ILogValueWriter>(typeof(double), new DefaultDoubleWriter()),
            new KeyValuePair<Type, ILogValueWriter>(typeof(float), new DefaultFloatWriter()),
            new KeyValuePair<Type, ILogValueWriter>(typeof(Guid), new DefaultGuidWriter()),
            new KeyValuePair<Type, ILogValueWriter>(typeof(int), new DefaultIntWriter()),
            new KeyValuePair<Type, ILogValueWriter>(typeof(uint), new DefaultUIntWriter()),
            new KeyValuePair<Type, ILogValueWriter>(typeof(long), new DefaultLongWriter()),
            new KeyValuePair<Type, ILogValueWriter>(typeof(string), new DefaultStringWriter()),
            new KeyValuePair<Type, ILogValueWriter>(typeof(TimeSpan), new DefaultTimeSpanWriter()),
            new KeyValuePair<Type, ILogValueWriter>(typeof(Actor), ActorLogWriter.Instance),
            new KeyValuePair<Type, ILogValueWriter>(typeof(Asset), AssetLogWriter.Instance),
            new KeyValuePair<Type, ILogValueWriter>(typeof(ActorId), ActorIdLogWriter.Instance),
            new KeyValuePair<Type, ILogValueWriter>(typeof(AssetId), AssetIdLogWriter.Instance),
            new KeyValuePair<Type, ILogValueWriter>(typeof(Money), new DefaultMoneyWriter()),
        ], ReferenceComparer<Type>.Instance);

        _valueFactories = new Queue<ILogValueWriterFactory>(4);
        _valueFactories.Enqueue(ActorLogWriter.Factory);
        _valueFactories.Enqueue(ActorIdLogWriter.Factory);
        _valueFactories.Enqueue(AssetLogWriter.Factory);
        _valueFactories.Enqueue(AssetIdLogWriter.Factory);
    }

    internal LogService Build()
    {
        return new LogService(_writers.ToArray());
    }

    public LogBuilder MinimalLevel(LogLevel level)
    {
        _level = level;
        return this;
    }

    public LogBuilder SetValueWriter<T>(ILogValueWriter<T> writer)
    {
        // for replace exists writer
        _valueWriters[typeof(T)] = writer;
        return this;
    }

    public LogBuilder SetValueWriterFactory(ILogValueWriterFactory writerFactory)
    {
        _valueFactories.Enqueue(writerFactory);
        return this;
    }

    public LogBuilder UseConsoleSink(int bufferSize = 16384, Encoding? encoding = null)
    {
        encoding ??= Console.OutputEncoding;
        var output = new StreamWriter(Console.OpenStandardOutput(), encoding, bufferSize);

        UseTextSink(output);

        return this;
    }

    public LogBuilder UseTextSink(StreamWriter writer)
    {
        var factories = _valueFactories.ToArray();
        var sink = new TextSink(_level, factories, _valueWriters, writer);

        return UseSink(sink);
    }

    public LogBuilder UseSink(ILogSink sink)
    {
        _writers.Add(sink);
        return this;
    }
}