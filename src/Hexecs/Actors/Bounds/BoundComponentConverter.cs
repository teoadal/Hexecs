using System.Text.Json;
using Hexecs.Actors.Serializations;
using Hexecs.Serializations;

namespace Hexecs.Actors.Bounds;

internal sealed class BoundComponentConverter : IActorComponentConverter<BoundComponent>
{
    public static readonly BoundComponentConverter Instance = new();
    
    public BoundComponent Deserialize(ActorContext context, ref Utf8JsonReader reader)
    {
        throw new NotImplementedException();
    }

    public void Serialize(ActorContext context, Utf8JsonWriter writer, in BoundComponent component)
    {
        writer.WriteProperty(nameof(BoundComponent.AssetId), component.AssetId);
    }
}