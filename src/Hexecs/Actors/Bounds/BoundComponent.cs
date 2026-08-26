using Hexecs.Actors.Components;
using Hexecs.Assets;

namespace Hexecs.Actors.Bounds;

/// <summary>
/// Компонент, связывающий актёра с идентификатором ассета
/// </summary>
/// <remarks>
/// Реализует интерфейс <see cref="IActorComponent"/> для интеграции с системой актёров
/// </remarks>
[DebuggerDisplay("Asset: {ToString()}")]
[DebuggerTypeProxy(typeof(BoundComponentDebugProxy))]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
internal readonly struct BoundComponent(AssetId assetId) : IActorComponent
{
    public static ActorComponentConfiguration<BoundComponent> CreatePoolConfiguration()
    {
        return new ActorComponentConfiguration<BoundComponent>(
            null,
            null,
            null,
            BoundComponentConverter.Instance);
    }

    /// <summary>
    /// Идентификатор ресурса (<see cref="Asset"/>), привязанного к актёру
    /// </summary>
    public readonly AssetId AssetId = assetId;

    public override string ToString()
    {
        return AssetMarshal.TryGetDebugContext(out var context)
            ? context.GetDescription(AssetId)
            : AssetId.IsEmpty
                ? StringUtils.EmptyValue
                : AssetId.ToString();
    }
}