using System.Text.Json;
using Hexecs.Actors;
using Hexecs.Actors.Serializations;
using Hexecs.Serializations;

namespace Hexecs.Monogame.Features.Healths;

internal sealed class HealthConverter : IActorComponentConverter<Health>
{
    public Health Deserialize(ActorContext context, ref Utf8JsonReader reader)
    {
        throw new NotImplementedException();
    }

    public void Serialize(ActorContext context, Utf8JsonWriter writer, in Health component)
    {
        writer.WriteProperty(nameof(Health.Value), component.Value);
    }
}