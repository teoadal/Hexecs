using Hexecs.Tests.Mocks.ActorComponents;

namespace Hexecs.Tests.Actors;

public sealed class ActorFilter2Should(ActorTestFixture fixture) : IClassFixture<ActorTestFixture>
{
    [Fact(DisplayName = "Возвращает пустой результат, если актеры не соответствуют фильтру")]
    public void ReturnsEmptyWhenNoActorsMatchFilter()
    {
        // arrange

        Actor expected = fixture.CreateActor<Defence>(); // Создаем актера с другим компонентом, чтобы убедиться, что он не попадет в фильтр

        // act
        ActorFilter<Attack> filter = fixture
            .Actors
            .Filter<Attack>(); // Фильтруем по Attack

        // assert
        Actor[] actors = filter.ToArray();
        actors
            .Should()
            .NotContain(actor => actor.Id == expected.Id);
    }

    [Fact(DisplayName = "Содержит актера с двумя компонентами при фильтрации по двум компонентам")]
    public void ContainsActorWithTwoComponentsWhenFilteredByTwoComponents()
    {
        // arrange
        var attackComponent = fixture.CreateComponent<Attack>();
        var defenceComponent = fixture.CreateComponent<Defence>();
        Actor actor = fixture.CreateActor<Attack, Defence>(component1: attackComponent, component2: defenceComponent);

        // act
        ActorFilter<Attack, Defence> filter = fixture
            .Actors
            .Filter<Attack, Defence>();

        // assert
        filter
            .Contains(actor)
            .Should()
            .BeTrue();

        ActorRef<Attack, Defence> actorRef = filter.GetRef(actor.Id);
        actorRef.Component1.Should().Be(attackComponent);
        actorRef.Component2.Should().Be(defenceComponent);
    }

    [Fact(DisplayName = "Не содержит актера только с одним из двух компонентов при фильтрации по двум компонентам")]
    public void DoesNotContainActorWithOnlyOneOfTwoComponentsWhenFilteredByTwoComponents()
    {
        // arrange
        var attackComponent = fixture.CreateComponent<Attack>();
        var defenceComponent = fixture.CreateComponent<Defence>();

        Actor actorWithBoth = fixture.CreateActor<Attack, Defence>(component1: attackComponent, component2: defenceComponent);
        Actor actorWithAttackOnly = fixture.CreateActor<Attack>(component1: attackComponent);
        Actor actorWithDefenceOnly = fixture.CreateActor<Defence>(component1: defenceComponent);

        // act
        ActorFilter<Attack, Defence> filter = fixture
            .Actors
            .Filter<Attack, Defence>();

        // assert
        filter.Contains(actorWithBoth).Should().BeTrue();
        filter.Contains(actorWithAttackOnly).Should().BeFalse();
        filter.Contains(actorWithDefenceOnly).Should().BeFalse();
    }

    [Fact(DisplayName = "Удаляет актера из фильтра по двум компонентам, если первый компонент (Attack) удален")]
    public void RemovesActorFromTwoComponentFilterWhenFirstComponentIsRemoved()
    {
        // arrange
        var attackComponent = fixture.CreateComponent<Attack>();
        var defenceComponent = fixture.CreateComponent<Defence>();
        Actor actor = fixture.CreateActor<Attack, Defence>(component1: attackComponent, component2: defenceComponent);

        ActorFilter<Attack, Defence> filter = fixture
            .Actors
            .Filter<Attack, Defence>();

        filter.Contains(actor).Should().BeTrue(); // Предусловие: актер в фильтре

        // act
        actor.Remove<Attack>();

        // assert
        filter.Contains(actor).Should().BeFalse();
    }

    [Fact(DisplayName = "Удаляет актера из фильтра по двум компонентам, если второй компонент (Defence) удален")]
    public void RemovesActorFromTwoComponentFilterWhenSecondComponentIsRemoved()
    {
        // arrange
        var attackComponent = fixture.CreateComponent<Attack>();
        var defenceComponent = fixture.CreateComponent<Defence>();
        Actor actor = fixture.CreateActor<Attack, Defence>(component1: attackComponent, component2: defenceComponent);

        ActorFilter<Attack, Defence> filter = fixture
            .Actors
            .Filter<Attack, Defence>();

        filter.Contains(actor).Should().BeTrue(); // Предусловие: актер в фильтре

        // act
        actor.Remove<Defence>();

        // assert
        filter.Contains(actor).Should().BeFalse();
    }
}
