using Hexecs.Actors;
using Hexecs.Monogame.Features.Planes.Create;
using Hexecs.Worlds;

namespace Hexecs.Monogame.Features.Planes;

internal static class PlaneInstaller
{
    public static ActorContextBuilder AddPlanes(this ActorContextBuilder builder)
    {
        return builder
            .AddBuilder<PlaneActorBuilder>()
            .CreateCommandHandler<CreatePlaneHandler>()
            .ConfigureComponentPool<Plane>(static pool => pool.CreateConverter<PlaneActorConverter>());
    }

    public static WorldBuilder ConfigurePlanes(this WorldBuilder builder)
    {
        return builder.AddAssetSource<PlaneAssetSource>();
    }
}