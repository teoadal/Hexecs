using Hexecs.Actors;

namespace Hexecs.Monogame.Features.Healths;

internal static class HealthInstaller
{
    public static ActorContextBuilder AddHealths(this ActorContextBuilder builder)
    {
        return builder
            .AddBuilder<HealthActorBuilder>()
            .ConfigureComponentPool<Health>(pool => pool.CreateConverter<HealthConverter>());;
    }
}