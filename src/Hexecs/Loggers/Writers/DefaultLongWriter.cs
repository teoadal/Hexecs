namespace Hexecs.Loggers.Writers;

internal sealed class DefaultLongWriter : ILogValueWriter<long>
{
    public void Write(ref ValueStringBuilder stringBuilder, long arg)
    {
        stringBuilder.Append(arg);
    }
}