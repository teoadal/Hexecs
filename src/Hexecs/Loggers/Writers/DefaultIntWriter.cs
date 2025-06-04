namespace Hexecs.Loggers.Writers;

internal sealed class DefaultIntWriter : ILogValueWriter<int>
{
    public void Write(ref ValueStringBuilder stringBuilder, int arg)
    {
        stringBuilder.Append(arg);
    }
}