using Hexecs.Dependencies;

namespace Hexecs.Worlds;

public static class WorldBuilderExtensions
{
    public static WorldBuilder UseSingleton<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces |
                                    DynamicallyAccessedMemberTypes.PublicConstructors)]
        TService>(this WorldBuilder builder) where TService : class
    {
        return builder.UseSingleton<TService>(ctx => (TService)ctx.Activate(typeof(TService)));
    }
}