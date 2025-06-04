using System.Text.Json;

namespace Hexecs.Actors.Relations;

internal interface IActorRelationPool
{
    ActorContext Context { get; }

    uint Id { get; }

    Type Type { get; }

    void Clear();

    int Count(uint subject);

    bool Has(uint subject, uint relative);

    bool Remove(uint subject);

    /// <summary>
    /// Сериализует содержимое пула отношений в формат JSON.
    /// </summary>
    /// <param name="writer">JSON-писатель для записи данных.</param>
    void Serialize(Utf8JsonWriter writer);
}