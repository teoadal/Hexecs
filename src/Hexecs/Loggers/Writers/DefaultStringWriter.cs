namespace Hexecs.Loggers.Writers;

internal sealed class DefaultStringWriter : ILogValueWriter<string>
{
    public void Write(ref ValueStringBuilder stringBuilder, string arg)
    {
        stringBuilder.Append(arg);
    }
}