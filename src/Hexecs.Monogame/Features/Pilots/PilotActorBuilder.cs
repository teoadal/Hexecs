using Hexecs.Actors;
using Hexecs.Assets;
using Hexecs.Utils;

namespace Hexecs.Monogame.Features.Pilots;

internal sealed class PilotActorBuilder : IActorBuilder<PilotAsset>
{
    public void Build(in Actor actor, in AssetRef<PilotAsset> asset, Args args)
    {
        actor.Add(new Pilot
        {
            Name = args.Get<string>(nameof(Pilot.Name)),
        });
    }
}