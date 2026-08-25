using Hexecs.Benchmarks.Map.Common.Positions;

namespace Hexecs.Benchmarks.Map.Common;

internal static class CommonInstaller
{
    public static ActorContextBuilder AddCommon(this ActorContextBuilder builder)
    {
        builder.AddPositions();

        return builder;
    }
}