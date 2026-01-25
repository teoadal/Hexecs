using Hexecs.Tests.Mocks.ActorComponents;

namespace Hexecs.Tests.Actors;

public sealed class ActorConstraintShould(ActorTestFixture fixture) : IClassFixture<ActorTestFixture>
{
    [Fact(DisplayName = "Должен успешно проходить проверку Applicable, если все условия соблюдены")]
    public void Should_Be_Applicable_When_Conditions_Met()
    {
        // Arrange
        var actor = fixture.CreateActor<Description>(); // Создаем актора с компонентом
        var constraint = ActorConstraint.Include<Description>(fixture.Actors).Build();

        // Act
        var result = constraint.Applicable(actor.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact(DisplayName = "Должен возвращать false в Applicable, если компонент исключен")]
    public void Should_Not_Be_Applicable_When_Excluded_Component_Exists()
    {
        // Arrange
        var actor = fixture.CreateActor<Description>();
        var constraint = ActorConstraint.Exclude<Description>(fixture.Actors).Build();

        // Act
        var result = constraint.Applicable(actor.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact(DisplayName = "Должен вызывать событие Added, когда актор начинает соответствовать фильтру")]
    public void Should_Raise_Added_When_Actor_Becomes_Valid()
    {
        // Arrange
        var actor = fixture.CreateActor(); // Пустой актор
        var constraint = ActorConstraint.Include<Description>(fixture.Actors).Build();

        using var monitoredSubject = constraint.Monitor();

        // Act
        actor.Add(new Description()); // Теперь актор соответствует Include<Description>

        // Assert
        monitoredSubject.Should().Raise(nameof(constraint.Added))
            .WithArgs<uint>(id => id == actor.Id);
    }

    [Fact(DisplayName = "Должен вызывать событие Removing, когда актор перестает соответствовать фильтру")]
    public void Should_Raise_Removing_When_Actor_Becomes_Invalid()
    {
        // Arrange
        var actor = fixture.CreateActor<Description>();
        var constraint = ActorConstraint.Include<Description>(fixture.Actors).Build();

        using var monitoredSubject = constraint.Monitor();

        // Act
        actor.Remove<Description>();

        // Assert
        monitoredSubject.Should().Raise(nameof(constraint.Removing))
            .WithArgs<uint>(id => id == actor.Id);
    }

    [Fact(DisplayName = "Builder должен выбрасывать исключение при добавлении дублирующегося компонента")]
    public void Builder_Should_Throw_On_Duplicate_Component()
    {
        // Arrange
        var builder = ActorConstraint.Include<Description>(fixture.Actors);

        // Act
        var action = () => builder.Include<Description>();

        // Assert
        action.Should().Throw<Exception>();
    }

    [Fact(DisplayName = "Два одинаковых ограничения должны иметь одинаковый HashCode и быть равны")]
    public void Should_Implement_Equality_Correctly()
    {
        // Arrange
        var constraint1 = ActorConstraint.Include<Description>(fixture.Actors)
            .Exclude<Defence>()
            .Build();

        var constraint2 = ActorConstraint.Include<Description>(fixture.Actors)
            .Exclude<Defence>()
            .Build();

        // Assert
        constraint1
            .Should()
            .Be(constraint2);

        constraint1.GetHashCode()
            .Should()
            .Be(constraint2.GetHashCode());
    }

    [Fact(DisplayName = "Должен корректно работать с несколькими Include компонентами")]
    public void Should_Work_With_Multiple_Includes()
    {
        // Arrange
        var actor = fixture.CreateActor<Description>();
        var constraint = ActorConstraint.Include<Description, Defence>(fixture.Actors).Build();

        // Act & Assert
        constraint.Applicable(actor.Id).Should().BeFalse("Нужны оба компонента");

        actor.Add(new Defence());
        constraint.Applicable(actor.Id).Should().BeTrue("Теперь оба компонента на месте");
    }

    [Fact(DisplayName = "Должен отписаться от всех событий пула при Dispose")]
    public void Should_Unsubscribe_On_Dispose()
    {
        // Arrange
        var actor = fixture.CreateActor();
        var constraint = ActorConstraint.Include<Description>(fixture.Actors).Build();

        using var monitoredSubject = constraint.Monitor();

        // Act
        ((IDisposable)constraint).Dispose();
        actor.Add(new Description());

        // Assert
        monitoredSubject.Should().NotRaise(nameof(constraint.Added));
    }
}