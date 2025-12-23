namespace Hexecs.Benchmarks.Map.Common.Positions;

internal static class PositionsInstaller
{
    public static ActorContextBuilder AddPositions(this ActorContextBuilder builder)
    {
        builder.AddBuilder<PositionBuilder>();
        return builder;
    }
}