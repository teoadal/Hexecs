namespace Hexecs.Loggers.Writers;

internal sealed class DefaultDoubleWriter : ILogValueWriter<double>
{
    public void Write(ref ValueStringBuilder stringBuilder, double arg)
    {
        stringBuilder.Append(arg);
    }
}