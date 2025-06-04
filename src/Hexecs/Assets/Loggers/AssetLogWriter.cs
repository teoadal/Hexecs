using Hexecs.Loggers;

namespace Hexecs.Assets.Loggers;

internal sealed class AssetLogWriter : ILogValueWriter<Asset>, ILogValueWriterFactory
{
    public static readonly AssetLogWriter Instance = new();
    public static ILogValueWriterFactory Factory => Instance;

    private AssetLogWriter()
    {
    }
    
    public bool TryCreateWriter<T>(out ILogValueWriter<T> writer)
    {
        var type = typeof(T);
        if (type is { IsValueType: true, IsGenericType: true })
        {
            if (type.GetGenericTypeDefinition() == typeof(Asset<>))
            {
                writer = new LikeAssetStruct<T>();
                return true;
            }
        }

        writer = null!;
        return false;
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
    
    private sealed class LikeAssetStruct<T> : ILogValueWriter<T>
    {
        public void Write(ref ValueStringBuilder stringBuilder, T arg)
        {
            ref readonly var actor = ref Unsafe.As<T, Asset>(ref arg);
            Instance.Write(ref stringBuilder, actor);           
        }
    }
}