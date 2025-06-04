namespace Hexecs.Assets;

/// <summary>
/// Интерфейс фильтра ассетов
/// </summary>
public interface IAssetFilter
{
    /// <summary>
    /// Ограничение на фильтрацию ассетов
    /// </summary>
    AssetConstraint? Constraint { get; }

    /// <summary>
    /// Контекст ассетов фильтра, управляющий их жизненным циклом и содержащий коллекции их компонентов.
    /// </summary>
    AssetContext Context { get; }

    /// <summary>
    /// Количество ассетов в фильтре
    /// </summary>
    int Length { get; }

    bool Contains(uint actorId);
}