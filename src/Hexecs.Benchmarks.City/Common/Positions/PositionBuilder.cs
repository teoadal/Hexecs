namespace Hexecs.Benchmarks.Map.Common.Positions;

internal sealed class PositionBuilder : IActorBuilder<PositionAbility>
{
    public void Build(in Actor actor, in AssetRef<PositionAbility> asset, Args args)
    {
        actor.Add(new Position
        {
            Grid = args.Get<Point>(nameof(Point))
        });
    }
}