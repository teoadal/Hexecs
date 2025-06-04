namespace Hexecs.Loggers.Writers;

internal sealed class DefaultFloatWriter : ILogValueWriter<float>
{
    public void Write(ref ValueStringBuilder stringBuilder, float arg)
    {
        stringBuilder.Append(arg);
    }
}