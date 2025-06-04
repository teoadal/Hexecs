using System.Globalization;
using Hexecs.Utils;

namespace Hexecs.Loggers.Writers;

internal sealed class DefaultMoneyWriter : ILogValueWriter<Money>
{
    public void Write(ref ValueStringBuilder stringBuilder, Money arg)
    {
        stringBuilder.Append(arg.Value, "N2", CultureInfo.InvariantCulture);
    }
}