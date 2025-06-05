using Hexecs.Actors;
using Hexecs.Monogame.Features.Healths;
using Hexecs.Monogame.Features.Pilots;
using Hexecs.Worlds;

namespace Hexecs.Monogame.Features;

internal static class FeatureInstaller
{
    public static ActorContextBuilder AddFeatures(this ActorContextBuilder builder) => builder
        .AddHealths()
        .AddPilots();

    public static WorldBuilder ConfigureFeatures(this WorldBuilder builder) => builder
        .ConfigurePilots();
}