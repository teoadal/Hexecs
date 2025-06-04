using Hexecs.Actors.Bounds;
using Hexecs.Assets;

namespace Hexecs.Actors;

public sealed partial class ActorContext
{
    /// <summary>
    /// Получает ассет, привязанный к актёру.
    /// </summary>
    /// <param name="actorId">Идентификатор актёра</param>
    /// <returns>Ассет, привязанный к актёру</returns>
    /// <exception cref="Exception">Возникает, если актёр не имеет привязанного ассета</exception>
    public Asset GetBoundAsset(uint actorId)
    {
        ref var component = ref TryGetComponentRef<BoundComponent>(actorId);
        if (Unsafe.IsNullRef(ref component)) ActorError.AssetNotFound(actorId);

        return World.Assets.GetAsset(component.AssetId);
    }

    /// <summary>
    /// Пытается получить ассет, привязанный к актёру.
    /// </summary>
    /// <param name="actorId">Идентификатор актёра</param>
    /// <param name="asset">Переменная для сохранения найденного ассета</param>
    /// <returns>Возвращает true, если актёр имеет привязанный ассет, иначе false</returns>
    public bool TryGetBoundAsset(uint actorId, out Asset asset)
    {
        ref var component = ref TryGetComponentRef<BoundComponent>(actorId);
        if (Unsafe.IsNullRef(ref component))
        {
            asset = Asset.Empty;
            return false;
        }

        asset = World.Assets.GetAsset(component.AssetId);
        return true;
    }

    /// <summary>
    /// Устанавливает ассет, привязанный к актёру.
    /// </summary>
    /// <param name="actorId">Идентификатор актёра</param>
    /// <param name="asset">Ассет для привязки</param>
    internal void SetBoundAsset(uint actorId, in Asset asset)
    {
        var pool = GetOrCreateComponentPool<BoundComponent>();
        if (asset.IsEmpty)
        {
            pool.Remove(actorId);
        }
        else
        {
            var component = new BoundComponent(asset.Id);
            if (!pool.Update(actorId, component))
            {
                pool.Add(actorId, component);
            }
        }
    }
}