using Hexecs.Actors;
using Hexecs.Assets;
using Hexecs.Utils;

namespace Hexecs.Monogame.Features.Planes;

internal sealed class PlaneActorBuilder : IActorBuilder<PlaneAsset>
{
    public void Build(in Actor actor, in AssetRef<PlaneAsset> asset, Args args)
    {
        actor.Add(new Plane
        {
            Name = args.Get<string>(nameof(Plane.Name)),
        });
    }
}