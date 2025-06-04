using Hexecs.Tests.Mocks;

namespace Hexecs.Tests.Actors;

public sealed class ActorFilter3Should(ActorTestFixture fixture) : IClassFixture<ActorTestFixture>
{
    [Fact(DisplayName = "Содержит актера с тремя компонентами при фильтрации по трем компонентам")]
    public void ContainsActorWithThreeComponentsWhenFilteredByThreeComponents()
    {
        // arrange
        var attackComponent = fixture.CreateComponent<Attack>();
        var defenceComponent = fixture.CreateComponent<Defence>();
        var speedComponent = fixture.CreateComponent<Speed>(); // Предполагаемый третий компонент
        var actor = fixture.CreateActor<Attack, Defence, Speed>(
            component1: attackComponent,
            component2: defenceComponent,
            component3: speedComponent);

        // act
        var filter = fixture
            .Actors
            .Filter<Attack, Defence, Speed>();

        // assert
        filter
            .Contains(actor)
            .Should()
            .BeTrue();

        var actorRef = filter.GetRef(actor.Id);
        actorRef.Component1
            .Should()
            .Be(attackComponent);

        actorRef.Component2
            .Should()
            .Be(defenceComponent);

        actorRef.Component3
            .Should()
            .Be(speedComponent);
    }

    [Fact(DisplayName = "Не содержит актера только с двумя из трех компонентов при фильтрации по трем компонентам")]
    public void DoesNotContainActorWithOnlyTwoOfThreeComponentsWhenFilteredByThreeComponents()
    {
        // arrange
        var attackComponent = fixture.CreateComponent<Attack>();
        var defenceComponent = fixture.CreateComponent<Defence>();
        var speedComponent = fixture.CreateComponent<Speed>();

        var actorWithAllThree = fixture.CreateActor<Attack, Defence, Speed>(component1: attackComponent,
            component2: defenceComponent, component3: speedComponent);
        var actorWithAttackDefence =
            fixture.CreateActor<Attack, Defence>(component1: attackComponent, component2: defenceComponent);
        var actorWithAttackSpeed =
            fixture.CreateActor<Attack, Speed>(component1: attackComponent,
                component2: speedComponent); // Используем component2 для Speed если CreateActor так работает
        var actorWithDefenceSpeed =
            fixture.CreateActor<Defence, Speed>(component1: defenceComponent, component2: speedComponent);


        // act
        var filter = fixture
            .Actors
            .Filter<Attack, Defence, Speed>();

        // assert
        filter.Contains(actorWithAllThree).Should().BeTrue();
        filter.Contains(actorWithAttackDefence).Should().BeFalse();
        filter.Contains(actorWithAttackSpeed).Should().BeFalse();
        filter.Contains(actorWithDefenceSpeed).Should().BeFalse();
    }

    [Fact(DisplayName = "Удаляет актера из фильтра по трем компонентам, если один компонент (например, Attack) удален")]
    public void RemovesActorFromThreeComponentFilterWhenAComponentIsRemoved()
    {
        // arrange
        var attackComponent = fixture.CreateComponent<Attack>();
        var defenceComponent = fixture.CreateComponent<Defence>();
        var speedComponent = fixture.CreateComponent<Speed>();
        var actor = fixture.CreateActor<Attack, Defence, Speed>(component1: attackComponent,
            component2: defenceComponent, component3: speedComponent);

        var filter = fixture
            .Actors
            .Filter<Attack, Defence, Speed>();

        filter.Contains(actor).Should().BeTrue(); // Предусловие: актер в фильтре

        // act
        actor.Remove<Attack>();

        // assert
        filter.Contains(actor).Should().BeFalse();
    }

    [Fact(DisplayName =
        "Добавляет актера в фильтр по трем компонентам, когда недостающий третий компонент (Speed) добавлен")]
    public void AddsActorToThreeComponentFilterWhenThirdComponentIsAdded()
    {
        // arrange
        var attackComponent = fixture.CreateComponent<Attack>();
        var defenceComponent = fixture.CreateComponent<Defence>();

        var actor = fixture.CreateActor<Attack, Defence>(component1: attackComponent, component2: defenceComponent);

        var filter = fixture
            .Actors
            .Filter<Attack, Defence, Speed>();

        filter.Contains(actor).Should().BeFalse(); // Предусловие: актер НЕ в фильтре

        var speedComponent = fixture.CreateComponent<Speed>();

        // act
        actor.Add(speedComponent);

        // assert
        filter.Contains(actor).Should().BeTrue();

        var actorRef = filter.GetRef(actor.Id);
        actorRef.Component1.Should().Be(attackComponent);
        actorRef.Component2.Should().Be(defenceComponent);
        actorRef.Component3.Should().Be(speedComponent);
    }
}