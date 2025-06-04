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
            .WriteProperty(nameof(Length), Length)
            .WriteProperty(nameof(IActorComponentPool.Type), Type)
            .WriteProperty("Instances", SerializeInstances);

        writer.WriteEndObject();
    }

    private void SerializeInstances(Utf8JsonWriter writer)
    {
        writer.WriteStartArray();

        var index = 0;
        var length = _length;
        var entries = _entries;
        while ((uint)index < (uint)length)
        {
            ref readonly var entry = ref entries[index];
            if (entry.Next >= -1)
            {
                writer.WriteStartObject();
                writer.WriteProperty("Owner", entry.Key);

                writer.WritePropertyName("Data");
                writer.WriteStartObject();

                _converter?.Serialize(Context, writer, in entry.Value);

                writer.WriteEndObject();
                writer.WriteEndObject();
            }

            index++;
        }

        writer.WriteEndArray();
        writer.Flush();
    }
}