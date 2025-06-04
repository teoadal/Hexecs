using Hexecs.Loggers;

namespace Hexecs.Assets.Loggers;

internal sealed class AssetIdLogWriter : ILogValueWriter<AssetId>, ILogValueWriterFactory
{
    public static readonly AssetIdLogWriter Instance = new();
    public static ILogValueWriterFactory Factory => Instance;

    private AssetIdLogWriter()
    {
    }

    public bool TryCreateWriter<T>(out ILogValueWriter<T> writer)
    {
        var type = typeof(T);
        if (type is { IsValueType: true, IsGenericType: true })
        {
            if (type.GetGenericTypeDefinition() == typeof(AssetId<>))
            {
                writer = new LikeAssetIdStruct<T>();
                return true;
            }
        }

        writer = null!;
        return false;
    }

    public void Write(ref ValueStringBuilder stringBuilder, AssetId asset)
    {
        if (asset.IsEmpty)
        {
            stringBuilder.Append(StringUtils.EmptyValue);
        }
        else
        {
            if (AssetMarshal.TryGetDebugContext(out var context))
            {
                context.GetDescription(asset.Value, ref stringBuilder);
            }
            else
            {
                stringBuilder.Append(asset.Value);
            }
        }
    }
    
    private sealed class LikeAssetIdStruct<T> : ILogValueWriter<T>
    {
        public void Write(ref ValueStringBuilder stringBuilder, T arg)
        {
            ref readonly var asset = ref Unsafe.As<T, AssetId>(ref arg);
            Instance.Write(ref stringBuilder, asset);
        }
    }
}