using System.Globalization;

namespace Hexecs.Loggers.Writers;

internal sealed class DefaultValueWriter<T> : ILogValueWriter<T>
{
    [SkipLocalsInit]
    public void Write(ref ValueStringBuilder stringBuilder, T arg)
    {
        // ReSharper disable once MergeCastWithTypeCheck
        if (arg is ISpanFormattable)
        {
            Span<char> buffer = stackalloc char[512];
            if (((ISpanFormattable)arg).TryFormat(buffer,
                    out var charsWritten,
                    ReadOnlySpan<char>.Empty,
                    CultureInfo.InvariantCulture))
            {
                stringBuilder.Append(buffer[..charsWritten]);
            }
            else
            {
                stringBuilder.Append(arg.ToString());
            }
        }
        else
        {
            stringBuilder.Append(arg == null ? "NULL" : arg.ToString());
        }
    }
}