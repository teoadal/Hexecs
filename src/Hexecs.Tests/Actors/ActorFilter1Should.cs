using Hexecs.Tests.Mocks;
using Hexecs.Utils;

namespace Hexecs.Tests.Actors;

public sealed class ActorFilter1Should(ActorTestFixture fixture) : IClassFixture<ActorTestFixture>
{
    [Fact(DisplayName = "Содержать актёра, который был создан до запроса")]
    public void ContainsActorCreatedBeforeFilter()
    {
        // arrange

        var component = fixture.CreateComponent<Attack>();
        Actor<Attack> actor = fixture.CreateActor<Attack>(component1: component);
        
        // act

        var filter = fixture
            .Actors
            .Filter<Attack>();

        // assert

        filter
            .Contains(actor)
            .Should()
            .BeTrue();

        filter
            .GetRef(actor.Id)
            .Component1
            .Should()
            .Be(component);
    }

    [Fact(DisplayName = "Содержать актёра, который был создан после запроса")]
    public void ContainsActorCreatedAfterFilter()
    {
        // arrange

        var component = fixture.CreateComponent<Attack>();
        var filter = fixture
            .Actors
            .Filter<Attack>();

        // act

        var actor = fixture.CreateActor<Attack>(component1: component);

        // assert

        filter
            .Contains(actor)
            .Should()
            .BeTrue();

        filter
            .GetRef(actor.Id)
            .Component1
            .Should()
            .Be(component);
    }

    [Fact(DisplayName = "Содержать всех добавленных актёров")]
    public void ContainsAllActors()
    {
        // arrange

        var actors = fixture.CreateActors<Attack, Defence>();

        var filter = fixture
            .Actors
            .Filter<Attack>();

        actors
            .Where(actor => actor.Id % 2 == 0)
            .Do(actor => actor.Destroy());

        var expectedActors = actors.Where(actor => actor.Id % 2 != 0);

        // act

        var actualActors = filter.ToArray();

        // assert

        actualActors
            .Should()
            .Contain(expectedActors);
    }

    [Fact(DisplayName = "Не содержать актёра, который был удалён")]
    public void RemoveDestroyedActor()
    {
        // arrange

        var actor = fixture.CreateActor<Attack>();
        var filter = fixture
            .Actors
            .Filter<Attack>();

        // act

        actor.Destroy();

        // assert

        filter
            .Contains(actor)
            .Should()
            .BeFalse();
    }

    [Fact(DisplayName = "Не содержать актёра, компонент которого был удалён")]
    public void RemoveActorAfterRemoveComponent()
    {
        // arrange

        var actor = fixture.CreateActor<Attack, Defence>();
        var filter = fixture
            .Actors
            .Filter<Attack>();

        // act

        actor.Remove<Attack>();

        // assert

        filter
            .Contains(actor)
            .Should()
            .BeFalse();
    }

    
}