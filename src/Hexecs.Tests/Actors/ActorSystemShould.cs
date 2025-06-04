using System.Collections.Concurrent;
using System.Diagnostics;
using Hexecs.Actors.Systems;
using Hexecs.Dependencies;
using Hexecs.Tests.Mocks;
using Hexecs.Threading;
using Hexecs.Worlds;

namespace Hexecs.Tests.Actors;

public sealed class ActorSystemShould(ActorTestFixture fixture) : IClassFixture<ActorTestFixture>
{
    [Fact(DisplayName = "Параллельные системы должны быть вызваны параллельно")]
    public void ConfigureAndRunSystemsInParallel()
    {
        var systems = fixture.CreateArray(_ => new Mock<IUpdateSystem>());

        using var world = new WorldBuilder()
            .DefaultParallelWorker(degreeOfParallelism: 4)
            .DefaultActorContext(cfg => cfg
                .CreateParallelUpdateSystem(parallel => parallel.Add(systems.Select(mock => mock.Object))))
            .Build();

        world.Update();
    }

    [Fact(DisplayName = "Параллельная система должны параллельно обработать всех акторов")]
    public void UpdateActorsInParallel()
    {
        var expectedValue = fixture.RandomInt();
        
        
        using var world = new WorldBuilder()
            .DefaultParallelWorker(degreeOfParallelism: 4)
            .DefaultActorContext(cfg => cfg
                .CreateParallelUpdateSystem(parallel =>
                    parallel.Create(ctx => new ParallelUpdateSystem(
                        expectedValue,
                        ctx,
                        ctx.GetRequiredService<IParallelWorker>()))))
            .Build();

        fixture.CreateActors<Attack, Defence, Speed>(100_000);

        for (var i = 0; i < 120; i++)
        {
            world.Update();
        }

        foreach (var actor in world.Actors.Filter<Defence, Attack, Speed>())
        {
            actor.Component1.Value.Should().Be(1);
            actor.Component2.Value.Should().Be(1);
            actor.Component3.Value.Should().Be(1);
        }
    }

    private sealed class ParallelUpdateSystem(
        int expectedValue,
        ActorContext context,
        IParallelWorker parallelWorker)
        : UpdateSystem<Defence, Attack, Speed>(context, parallelWorker: parallelWorker)
    {
        protected override void Update(in ActorRef<Defence, Attack, Speed> actor, in WorldTime time)
        {
            actor.Component1.Value = expectedValue;
            actor.Component2.Value = expectedValue;
            actor.Component3.Value = expectedValue;
        }
    }
}