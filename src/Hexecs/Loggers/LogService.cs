namespace Hexecs.Loggers;

public sealed class LogService : ILogSink
{
    public static LogService Empty => new([]);

    private readonly ILogSink[] _writers;

    internal LogService(ILogSink[] writers)
    {
        _writers = writers;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ContextLogger CreateContext(string context) => new(this, context);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ContextLogger CreateContext(Type context) => new(this, TypeOf.GetTypeName(context));

    public bool IsEnabled(LogLevel level)
    {
        if (_writers.Length == 0) return false;

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var writer in _writers)
        {
            if (writer.IsEnabled(level)) return true;
        }

        return false;
    }

    public void Write(LogLevel level, string context, string template)
    {
        foreach (var writer in _writers)
        {
            writer.Write(level, context, template);
        }
    }

    public void Write<T1>(LogLevel level, string context, string template, T1 arg1)
    {
        foreach (var writer in _writers)
        {
            writer.Write(level, context, template, arg1);
        }
    }

    public void Write<T1, T2>(LogLevel level, string context, string template, T1 arg1, T2 arg2)
    {
        foreach (var writer in _writers)
        {
            writer.Write(level, context, template, arg1, arg2);
        }
    }

    public void Write<T1, T2, T3>(LogLevel level, string context, string template, T1 arg1, T2 arg2, T3 arg3)
    {
        foreach (var writer in _writers)
        {
            writer.Write(level, context, template, arg1, arg2, arg3);
        }
    }

    public void Write<T1, T2, T3, T4>(
        LogLevel level,
        string context, string template,
        T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        foreach (var writer in _writers)
        {
            writer.Write(level, context, template, arg1, arg2, arg3, arg4);
        }
    }

    public void Write<T1, T2, T3, T4, T5>(
        LogLevel level,
        string context, string template,
        T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        foreach (var writer in _writers)
        {
            writer.Write(level, context, template, arg1, arg2, arg3, arg4, arg5);
        }
    }

    public void Dispose()
    {
        foreach (var writer in _writers)
        {
            writer.Dispose();
        }
    }
}