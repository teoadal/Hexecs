namespace Hexecs.Benchmarks.Map.Common.Visibles;

internal static class VisibleInstaller
{
    public static ActorContextBuilder AddVisible(this ActorContextBuilder builder)
    {
        builder
            .ConfigureComponentPool<Visible>(terrain => terrain
                .Capacity(4096));

        builder.CreateUpdateSystem<VisibleSystem>();

        return builder;
    }
}