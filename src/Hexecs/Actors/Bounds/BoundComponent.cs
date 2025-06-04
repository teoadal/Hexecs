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
internal readonly struct BoundComponent(uint assetId) : IActorComponent
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
    public readonly uint AssetId = assetId;

    public override string ToString() => AssetMarshal.TryGetDebugContext(out var context)
        ? context.GetDescription(AssetId)
        : AssetId == Asset.EmptyId
            ? StringUtils.EmptyValue
            : AssetId.ToString();
}