using System.Text.Json;
using Hexecs.Actors.Components;
using Hexecs.Serializations;

namespace Hexecs.Actors.Relations;

internal sealed partial class ActorRelationPool<T>
{
    public void Serialize(Utf8JsonWriter writer)
    {
        writer.WriteStartObject();

        writer
            .WriteProperty(nameof(IActorRelationPool.Id), Id)
            .WriteProperty(nameof(IActorComponentPool.Type), Type)
            .WriteProperty("Instances", SerializeInstances);

        writer.WriteEndObject();
    }

    private void SerializeInstances(Utf8JsonWriter writer)
    {
        writer.WriteStartArray();

        writer.WriteEndArray();
        writer.Flush();
    }
}