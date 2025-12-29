using Hexecs.Actors.Systems;
using Hexecs.Dependencies;
using Hexecs.Tests.Mocks;
using Hexecs.Tests.Mocks.ActorComponents;
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
            .UseDefaultParallelWorker(degreeOfParallelism: 4)
            .UseDefaultActorContext(cfg => cfg
                .CreateParallelUpdateSystem(systems.Select(mock => mock.Object)))
            .Build();

        world.Update();
    }

    [Theory(DisplayName = "Параллельная система должны параллельно обработать всех акторов и только один раз")]
    [InlineData(2, 1)]
    [InlineData(2, 2)]
    [InlineData(2, 3)]
    [InlineData(2, 999)]
    [InlineData(2, 1000)]
    [InlineData(2, 1001)]
    [InlineData(3, 1)]
    [InlineData(3, 2)]
    [InlineData(3, 3)]
    [InlineData(3, 999)]
    [InlineData(3, 1000)]
    [InlineData(3, 1001)]
    public void UpdateActorsInParallel(int degreeOfParallelism, int actorCount)
    {
        // arrange

        using var world = new WorldBuilder()
            .UseDefaultParallelWorker(degreeOfParallelism)
            .UseDefaultActorContext(cfg => cfg
                .CreateUpdateSystem(ctx => new ParallelUpdateSystem(
                    ctx,
                    ctx.GetRequiredService<IParallelWorker>())))
            .Build();

        var actorContext = world.Actors;
        for (uint i = 1; i <= actorCount; i++)
        {
            var actor = actorContext.CreateActor(i);
            actor.Add(new Attack { Value = 0 });
            actor.Add(new Defence { Value = 0 });
            actor.Add(new Speed { Value = 0 });
        }

        // act

        world.Update();

        // assert


        var actorFilter = actorContext.Filter<Defence, Attack, Speed>();

        actorFilter.Length
            .Should()
            .Be(actorCount);
        
        foreach (var actor in actorFilter)
        {
            actor.Component1.Value
                .Should()
                .Be(1,
                    "Component {0} value of actor {1} should be updated to 1",
                    actor.Component1.GetType().Name,
                    actor.Id);

            actor.Component2.Value
                .Should()
                .Be(1,
                    "Component {0} value of actor {1} should be updated to 1",
                    actor.Component1.GetType().Name,
                    actor.Id);

            actor.Component3.Value
                .Should()
                .Be(1,
                    "Component {0} value of actor {1} should be updated to 1",
                    actor.Component1.GetType().Name,
                    actor.Id);
        }
    }

    private sealed class ParallelUpdateSystem(
        ActorContext context,
        IParallelWorker parallelWorker)
        : UpdateSystem<Defence, Attack, Speed>(context, parallelWorker: parallelWorker)
    {
        protected override void Update(in ActorRef<Defence, Attack, Speed> actor, in WorldTime time)
        {
            actor.Component1.Value += 1;
            actor.Component2.Value += 1;
            actor.Component3.Value += 1;
        }
    }
}