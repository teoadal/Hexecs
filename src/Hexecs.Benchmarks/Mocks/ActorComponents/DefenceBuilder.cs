using Hexecs.Assets;
using Hexecs.Utils;

namespace Hexecs.Benchmarks.Mocks.ActorComponents;

internal sealed class DefenceBuilder : IActorBuilder<UnitAsset>
{
    public void Build(in Actor actor, in AssetRef<UnitAsset> asset, Args args)
    {
        actor.Add(new Defence { Value = asset.Component1.Defence });
    }
}