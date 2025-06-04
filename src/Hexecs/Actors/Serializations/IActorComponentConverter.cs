using System.Text.Json;

namespace Hexecs.Actors.Serializations;

public interface IActorComponentConverter<T>
    where T : struct, IActorComponent
{
    T Deserialize(ActorContext context, ref Utf8JsonReader reader);

    void Serialize(ActorContext context, Utf8JsonWriter writer, in T component);
}