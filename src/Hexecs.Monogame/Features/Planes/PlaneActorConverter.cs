using System.Text.Json;
using Hexecs.Actors;
using Hexecs.Actors.Serializations;
using Hexecs.Serializations;

namespace Hexecs.Monogame.Features.Planes;

internal sealed class PlaneActorConverter : IActorComponentConverter<Plane>
{
    public Plane Deserialize(ActorContext context, ref Utf8JsonReader reader)
    {
        throw new NotImplementedException();
    }

    public void Serialize(ActorContext context, Utf8JsonWriter writer, in Plane component)
    {
        writer.WriteProperty(nameof(Plane.Name), component.Name);
    }
}