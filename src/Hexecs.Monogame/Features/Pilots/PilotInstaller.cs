using Hexecs.Actors;
using Hexecs.Monogame.Features.Pilots.Create;
using Hexecs.Worlds;

namespace Hexecs.Monogame.Features.Pilots;

internal static class PilotInstaller
{
    public static ActorContextBuilder AddPilots(this ActorContextBuilder builder)
    {
        return builder
            .AddBuilder<PilotActorBuilder>()
            .CreateCommandHandler<CreatePilotHandler>()
            .ConfigureComponentPool<Pilot>(static pool => pool.CreateConverter<PilotActorConverter>());
    }

    public static WorldBuilder ConfigurePilots(this WorldBuilder builder)
    {
        return builder.AddAssetSource<PilotAssetSource>();
    }
}