using System.Collections.Concurrent;
using Hexecs.Loggers.Writers;

namespace Hexecs.Loggers.Sinks;

internal sealed class TextSink : ILogSink, IAsyncDisposable
{
    private static List<TemplateSegment>? _segmentBuffer;

    private readonly ILogValueWriterFactory[] _logValueWriterFactories;
    private readonly LogLevel _minLevel;
    private readonly ConcurrentDictionary<string, TemplateSegment[]> _templates;
    private readonly ConcurrentDictionary<Type, ILogValueWriter> _valueWriters;
    private readonly TextWriter _writer;

    public TextSink(
        LogLevel minLevel,
        ILogValueWriterFactory[] logValueWriterFactories,
        ConcurrentDictionary<Type, ILogValueWriter> valueWriters,
        StreamWriter writer)
    {
        _logValueWriterFactories = logValueWriterFactories;
        _minLevel = minLevel;
        _templates = new ConcurrentDictionary<string, TemplateSegment[]>();
        _valueWriters = valueWriters;
        _writer = writer;
    }

    public void Dispose()
    {
        _writer.Flush();
        _writer.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _writer.FlushAsync();
        await _writer.DisposeAsync();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsEnabled(LogLevel level) => level >= _minLevel;

    [SkipLocalsInit]
    public void Write(LogLevel level, string context, string template)
    {
        if (!IsEnabled(level)) return;

        var sb = new ValueStringBuilder(stackalloc char[512]);

        BeginWrite(level, context, ref sb);
        sb.Append(template);
        EndWrite(ref sb);
    }

    [SkipLocalsInit]
    public void Write<T1>(LogLevel level, string context, string template, T1 arg1)
    {
        if (!IsEnabled(level)) return;

        var segments = _templates.GetOrAdd(template, CreateTemplate);
        var sb = new ValueStringBuilder(stackalloc char[512]);

        BeginWrite(level, context, ref sb);

        foreach (ref var segment in segments.AsSpan())
        {
            if (segment.IsArgument)
            {
                switch (segment.Index)
                {
                    case 1:
                        ExecuteValueWriter(ref sb, ref segment, arg1);
                        break;
                    default:
                        sb.Append(segment.Value.Span);
                        break;
                }
            }
            else
            {
                sb.Append(segment.Value.Span);
            }
        }

        EndWrite(ref sb);
    }

    [SkipLocalsInit]
    public void Write<T1, T2>(LogLevel level, string context, string template, T1 arg1, T2 arg2)
    {
        if (!IsEnabled(level)) return;

        var segments = _templates.GetOrAdd(template, CreateTemplate);
        var sb = new ValueStringBuilder(stackalloc char[512]);

        BeginWrite(level, context, ref sb);

        foreach (ref var segment in segments.AsSpan())
        {
            if (segment.IsArgument)
            {
                switch (segment.Index)
                {
                    case 1:
                        ExecuteValueWriter(ref sb, ref segment, arg1);
                        break;
                    case 2:
                        ExecuteValueWriter(ref sb, ref segment, arg2);
                        break;
                    default:
                        sb.Append(segment.Value.Span);
                        break;
                }
            }
            else
            {
                sb.Append(segment.Value.Span);
            }
        }

        EndWrite(ref sb);
    }

    [SkipLocalsInit]
    public void Write<T1, T2, T3>(LogLevel level, string context, string template, T1 arg1, T2 arg2, T3 arg3)
    {
        if (!IsEnabled(level)) return;

        var segments = _templates.GetOrAdd(template, CreateTemplate);
        var sb = new ValueStringBuilder(stackalloc char[512]);

        BeginWrite(level, context, ref sb);

        foreach (ref var segment in segments.AsSpan())
        {
            if (segment.IsArgument)
            {
                switch (segment.Index)
                {
                    case 1:
                        ExecuteValueWriter(ref sb, ref segment, arg1);
                        break;
                    case 2:
                        ExecuteValueWriter(ref sb, ref segment, arg2);
                        break;
                    case 3:
                        ExecuteValueWriter(ref sb, ref segment, arg3);
                        break;
                    default:
                        sb.Append(in segment.Value);
                        break;
                }
            }
            else
            {
                sb.Append(segment.Value.Span);
            }
        }

        EndWrite(ref sb);
    }

    [SkipLocalsInit]
    public void Write<T1, T2, T3, T4>(
        LogLevel level,
        string context, string template,
        T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        if (!IsEnabled(level)) return;

        var segments = _templates.GetOrAdd(template, CreateTemplate);
        var sb = new ValueStringBuilder(stackalloc char[512]);

        BeginWrite(level, context, ref sb);

        foreach (ref var segment in segments.AsSpan())
        {
            if (segment.IsArgument)
            {
                switch (segment.Index)
                {
                    case 1:
                        ExecuteValueWriter(ref sb, ref segment, arg1);
                        break;
                    case 2:
                        ExecuteValueWriter(ref sb, ref segment, arg2);
                        break;
                    case 3:
                        ExecuteValueWriter(ref sb, ref segment, arg3);
                        break;
                    case 4:
                        ExecuteValueWriter(ref sb, ref segment, arg4);
                        break;
                    default:
                        sb.Append(segment.Value.Span);
                        break;
                }
            }
            else
            {
                sb.Append(segment.Value.Span);
            }
        }

        EndWrite(ref sb);
    }

    public void Write<T1, T2, T3, T4, T5>(
        LogLevel level,
        string context, string template,
        T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        if (!IsEnabled(level)) return;

        var segments = _templates.GetOrAdd(template, CreateTemplate);
        var sb = new ValueStringBuilder(stackalloc char[512]);

        BeginWrite(level, context, ref sb);

        foreach (ref var segment in segments.AsSpan())
        {
            if (segment.IsArgument)
            {
                switch (segment.Index)
                {
                    case 1:
                        ExecuteValueWriter(ref sb, ref segment, arg1);
                        break;
                    case 2:
                        ExecuteValueWriter(ref sb, ref segment, arg2);
                        break;
                    case 3:
                        ExecuteValueWriter(ref sb, ref segment, arg3);
                        break;
                    case 4:
                        ExecuteValueWriter(ref sb, ref segment, arg4);
                        break;
                    case 5:
                        ExecuteValueWriter(ref sb, ref segment, arg5);
                        break;
                    default:
                        sb.Append(segment.Value.Span);
                        break;
                }
            }
            else
            {
                sb.Append(segment.Value.Span);
            }
        }

        EndWrite(ref sb);
    }

    private static void BeginWrite(LogLevel level, string context, ref ValueStringBuilder sb)
    {
        sb.Append('[');
        sb.Append(level switch
        {
            LogLevel.Trace => "TRC ",
            LogLevel.Debug => "DBG ",
            LogLevel.Info => "INF ",
            LogLevel.Warning => "WRN ",
            LogLevel.Error => "ERR ",
            _ => "TRC "
        });
        sb.Append(DateTime.Now, "HH:mm:ss");
        sb.Append(']');

        if (!string.IsNullOrEmpty(context))
        {
            sb.Whitespace();
            sb.Append(context);
            sb.Append(':');
        }

        sb.Whitespace();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteValueWriter<T>(ref ValueStringBuilder sb, ref TemplateSegment segment, T value)
    {
        ref var writer = ref segment.Writer;
        writer ??= GetValueWriter<T>();

        Unsafe
            .As<ILogValueWriter<T>>(writer)
            .Write(ref sb, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EndWrite(ref ValueStringBuilder sb)
    {
        _writer.WriteLine(sb.AsReadonlySpan());
        _writer.Flush();

        sb.Dispose();
    }

    private ILogValueWriter<T> GetValueWriter<T>()
    {
        var valueType = typeof(T);
        if (_valueWriters.TryGetValue(valueType, out var existsWriter))
        {
            return Unsafe.As<ILogValueWriter<T>>(existsWriter);
        }

        foreach (var valueWriterFactory in _logValueWriterFactories)
        {
            if (!valueWriterFactory.TryCreateWriter<T>(out var createdWriter)) continue;

            _valueWriters.TryAdd(valueType, createdWriter);
            return createdWriter;
        }

        var defaultWriter = new DefaultValueWriter<T>();
        _valueWriters.TryAdd(valueType, defaultWriter);
        return defaultWriter;
    }

    private static TemplateSegment[] CreateTemplate(string template)
    {
        var segments = Interlocked.Exchange(ref _segmentBuffer, null) ?? [];

        var segmentLength = 0;
        var segmentStartIndex = 0;
        byte segmentIndex = 1;

        for (var i = 0; i < template.Length; i++)
        {
            var ch = template[i];
            switch (ch)
            {
                case '{':
                    var textValue = template.AsMemory(segmentStartIndex, segmentLength);
                    segments.Add(new TemplateSegment(0, textValue));
                    segmentStartIndex = i + 1;
                    segmentLength = 0;
                    continue;
                case '}':
                    var argumentValue = template.AsMemory(segmentStartIndex, segmentLength);
                    segments.Add(new TemplateSegment(segmentIndex++, argumentValue));
                    segmentStartIndex = i + 1;
                    segmentLength = 0;
                    continue;
            }

            segmentLength++;
        }

        if (segmentLength > 0)
        {
            segments.Add(new TemplateSegment(0, template.AsMemory(segmentStartIndex, segmentLength)));
        }

        var result = segments.ToArray();

        segments.Clear();
        Interlocked.Exchange(ref _segmentBuffer, segments);

        return result;
    }

    [DebuggerDisplay("{Value} ")]
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    private struct TemplateSegment(byte index, ReadOnlyMemory<char> value)
    {
        public bool IsArgument
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Index != 0;
        }

        public readonly byte Index = index;
        public readonly ReadOnlyMemory<char> Value = value;
        public ILogValueWriter? Writer;
    }
}