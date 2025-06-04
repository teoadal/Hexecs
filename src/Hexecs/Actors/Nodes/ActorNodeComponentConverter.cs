using System.Text.Json;
using Hexecs.Actors.Serializations;

namespace Hexecs.Actors.Nodes;

internal sealed class ActorNodeComponentConverter : IActorComponentConverter<ActorNodeComponent>
{
    public static readonly ActorNodeComponentConverter Instance = new();
    
    public ActorNodeComponent Deserialize(ActorContext context, ref Utf8JsonReader reader)
    {
        throw new NotImplementedException();
    }

    public void Serialize(ActorContext context, Utf8JsonWriter writer, in ActorNodeComponent component)
    {
        
    }
}