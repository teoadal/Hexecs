using System.Text.Json;
using Hexecs.Actors.Serializations;

namespace Hexecs.Actors.Relations;

internal sealed class ActorRelationComponentConverter : IActorComponentConverter<ActorRelationComponent>
{
    public static readonly ActorRelationComponentConverter Instance = new ActorRelationComponentConverter();

    public ActorRelationComponent Deserialize(ActorContext context, ref Utf8JsonReader reader)
    {
        throw new NotImplementedException();
    }

    public void Serialize(ActorContext context, Utf8JsonWriter writer, in ActorRelationComponent component)
    {
        writer.WriteStartArray();

        foreach (uint relationId in component)
        {
            writer.WriteNumberValue(relationId);
        }

        writer.WriteEndArray();
    }
}
