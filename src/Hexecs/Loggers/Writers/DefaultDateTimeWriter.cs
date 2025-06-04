namespace Hexecs.Loggers.Writers;

internal sealed class DefaultDateTimeWriter : ILogValueWriter<DateTime>
{
    public void Write(ref ValueStringBuilder stringBuilder, DateTime arg)
    {
        stringBuilder.Append(arg);
    }
}