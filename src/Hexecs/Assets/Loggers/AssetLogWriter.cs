using Hexecs.Loggers;

namespace Hexecs.Assets.Loggers;

internal sealed class AssetLogWriter : ILogValueWriter<Asset>
{
    public static readonly AssetLogWriter Instance = new AssetLogWriter();

    private AssetLogWriter()
    {
    }

    public void Write(ref ValueStringBuilder stringBuilder, Asset asset)
    {
        if (asset.IsEmpty)
        {
            stringBuilder.Append(StringUtils.EmptyValue);
        }
        else
        {
            asset.Context.GetDescription(asset.Id, ref stringBuilder);
        }
    }
}
