namespace Hexecs.Loggers.Writers;

internal sealed class DefaultGuidWriter : ILogValueWriter<Guid>
{
    public void Write(ref ValueStringBuilder stringBuilder, Guid arg)
    {
        stringBuilder.Append(arg);
    }
}