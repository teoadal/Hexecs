using Hexecs.Loggers;

namespace Hexecs.Assets.Loggers;

internal sealed class AssetIdLogWriter : ILogValueWriter<AssetId>
{
    public static readonly AssetIdLogWriter Instance = new AssetIdLogWriter();

    private AssetIdLogWriter()
    {
    }

    public void Write(ref ValueStringBuilder stringBuilder, AssetId asset)
    {
        if (asset.IsEmpty)
        {
            stringBuilder.Append(StringUtils.EmptyValue);
        }
        else
        {
            if (AssetMarshal.TryGetDebugContext(out AssetContext? context))
            {
                context.GetDescription(asset, ref stringBuilder);
            }
            else
            {
                stringBuilder.Append(asset.Value);
            }
        }
    }
}
