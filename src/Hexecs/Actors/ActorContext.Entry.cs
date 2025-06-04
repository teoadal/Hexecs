using System.Text.Json;
using Hexecs.Collections;
using Hexecs.Serializations;

namespace Hexecs.Actors;

public sealed partial class ActorContext
{
    private struct Entry
    {
        public uint Key;
        public int Next;
        public InlineBucket<ushort> Components;

        public readonly void Serialize(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();

            writer.WriteProperty("Key", Key);
            writer.WriteProperty("Components", in Components);

            writer.WriteEndObject();
        }
    }
}