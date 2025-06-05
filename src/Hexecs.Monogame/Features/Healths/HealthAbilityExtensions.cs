using Hexecs.Assets.Sources;

namespace Hexecs.Monogame.Features.Healths;

public static class HealthAbilityExtensions
{
    public static AssetConfigurator Health(this in AssetConfigurator configurator, ushort value = 10000)
    {
        configurator.Set(new HealthAbility(value));
        return configurator;
    }
}