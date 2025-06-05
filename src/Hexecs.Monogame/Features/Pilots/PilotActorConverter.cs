using System.Text.Json;
using Hexecs.Actors;
using Hexecs.Actors.Serializations;
using Hexecs.Serializations;

namespace Hexecs.Monogame.Features.Pilots;

internal sealed class PilotActorConverter : IActorComponentConverter<Pilot>
{
    public Pilot Deserialize(ActorContext context, ref Utf8JsonReader reader)
    {
        throw new NotImplementedException();
    }

    public void Serialize(ActorContext context, Utf8JsonWriter writer, in Pilot component)
    {
        writer.WriteProperty(nameof(Pilot.Name), component.Name);
    }
}