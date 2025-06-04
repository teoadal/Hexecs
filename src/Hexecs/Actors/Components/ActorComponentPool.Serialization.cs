using System.Text.Json;
using Hexecs.Serializations;

namespace Hexecs.Actors.Components;

internal sealed partial class ActorComponentPool<T>
{
    public void Serialize(Utf8JsonWriter writer)
    {
        writer.WriteStartObject();

        writer
            .WriteProperty(nameof(IActorComponentPool.Id), Id)
            .WriteProperty(nameof(Length), _count) // Используем _count напрямую
            .WriteProperty(nameof(IActorComponentPool.Type), Type)
            .WriteProperty("Instances", SerializeInstances);

        writer.WriteEndObject();
    }

    private void SerializeInstances(Utf8JsonWriter writer)
    {
        writer.WriteStartArray();

        var count = _count;
        var keys = _dense;
        var values = _values;

        for (var i = 0; i < count; i++)
        {
            writer.WriteStartObject();
            writer.WriteProperty("Owner", keys[i]);

            writer.WritePropertyName("Data");
            writer.WriteStartObject();

            _converter?.Serialize(Context, writer, in values[i]);

            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.Flush();
    }
}