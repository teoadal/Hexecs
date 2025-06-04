namespace Hexecs.Loggers.Writers;

internal sealed class DefaultUIntWriter: ILogValueWriter<uint>
{
    public void Write(ref ValueStringBuilder stringBuilder, uint arg)
    {
        stringBuilder.Append(arg);
    }
}