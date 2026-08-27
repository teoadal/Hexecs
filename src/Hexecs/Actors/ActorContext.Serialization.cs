using System.Text.Json;

using Hexecs.Actors.Components;
using Hexecs.Actors.Relations;
using Hexecs.Serializations;

namespace Hexecs.Actors;

public sealed partial class ActorContext
{
    public void Serialize(Utf8JsonWriter writer)
    {
        writer.WriteStartObject();

        writer.WriteProperty(nameof(Id), Id)
            .WriteProperty(nameof(IsDefault), IsDefault)
            .WriteProperty(nameof(Length), Length)
            .WriteProperty("ComponentTypes", SerializeComponentTypes)
            .WriteProperty("RelationTypes", SerializeRelationTypes)
            .WriteProperty("Actors", SerializeEntries)
            .WriteProperty("Components", SerializeComponents)
            .WriteProperty("Relations", SerializeRelations);

        writer.WriteEndObject();
    }

    private void SerializeComponents(Utf8JsonWriter writer)
    {
        writer.WriteStartArray();

        foreach (IActorComponentPool? pool in _componentPools)
        {
            pool?.Serialize(writer);
        }

        writer.WriteEndArray();
    }

    private void SerializeComponentTypes(Utf8JsonWriter writer)
    {
        writer.WriteStartArray();

        foreach (IActorComponentPool? pool in _componentPools)
        {
            if (pool == null)
            {
                continue;
            }

            writer.WriteStartObject();

            writer
                .WriteProperty(nameof(IActorComponentPool.Id), pool.Id)
                .WriteProperty(nameof(IActorComponentPool.Type), pool.Type);

            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private void SerializeEntries(Utf8JsonWriter writer)
    {
        writer.WriteStartArray();

        int count = _count;
        uint[] dense = _dense;
        Entry[] values = _values;

        for (var i = 0; i < count; i++)
        {
            // Извлекаем ID из плотного массива ключей
            uint actorId = dense[i];

            // Получаем ссылку на данные по тому же индексу
            ref readonly Entry entry = ref values[i];

            entry.Serialize(actorId, writer);
        }

        writer.WriteEndArray();
    }

    private void SerializeRelations(Utf8JsonWriter writer)
    {
        writer.WriteStartArray();

        foreach (IActorRelationPool? pool in _relationPools)
        {
            pool?.Serialize(writer);
        }

        writer.WriteEndArray();
    }

    private void SerializeRelationTypes(Utf8JsonWriter writer)
    {
        writer.WriteStartArray();

        foreach (IActorRelationPool? pool in _relationPools)
        {
            if (pool == null)
            {
                continue;
            }

            writer.WriteStartObject();

            writer
                .WriteProperty(nameof(IActorRelationPool.Id), pool.Id)
                .WriteProperty(nameof(IActorRelationPool.Type), pool.Type);

            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }
}
