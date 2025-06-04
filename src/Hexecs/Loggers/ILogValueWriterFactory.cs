namespace Hexecs.Loggers;

public interface ILogValueWriterFactory
{
    bool TryCreateWriter<T>(out ILogValueWriter<T> writer);
}