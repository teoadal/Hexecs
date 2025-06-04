namespace Hexecs.Loggers.Writers;

internal sealed class DefaultTimeSpanWriter : ILogValueWriter<TimeSpan>
{
    public void Write(ref ValueStringBuilder stringBuilder, TimeSpan arg)
    {
        stringBuilder.Append(arg);
    }
}