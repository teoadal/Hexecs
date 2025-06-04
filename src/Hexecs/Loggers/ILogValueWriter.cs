namespace Hexecs.Loggers;

public interface ILogValueWriter;

public interface ILogValueWriter<in T> : ILogValueWriter
{
    void Write(ref ValueStringBuilder stringBuilder, T arg);
}