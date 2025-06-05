using Hexecs.Actors;
using Hexecs.Assets;
using Hexecs.Utils;

namespace Hexecs.Monogame.Features.Healths;

internal sealed class HealthActorBuilder : IActorBuilder<HealthAbility>
{
    public void Build(in Actor actor, in AssetRef<HealthAbility> asset, Args args)
    {
        actor.Add(new Health { Value = asset.Component1.Value });
    }
}