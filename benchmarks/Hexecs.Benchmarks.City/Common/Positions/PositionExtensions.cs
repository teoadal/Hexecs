using Hexecs.Assets.Sources;

namespace Hexecs.Benchmarks.Map.Common.Positions;

internal static class PositionExtensions
{
    public static AssetConfigurator WithPosition(this AssetConfigurator configurator)
    {
        configurator.Set(new PositionAbility());
        return configurator;
    }
}