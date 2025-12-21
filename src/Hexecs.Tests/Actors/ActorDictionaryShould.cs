using Hexecs.Tests.Mocks;

namespace Hexecs.Tests.Actors;

public sealed class ActorDictionaryShould(ActorTestFixture fixture) : IClassFixture<ActorTestFixture>
{
    [Fact(DisplayName = "Должен автоматически добавлять актора в словарь при добавлении компонента")]
    public void Should_Add_Actor_When_Component_Added()
    {
        // Arrange
        // Используем Name как ключ
        using var dict = new ActorDictionary<string, Description>(
            fixture.Actors,
            c => c.Name);

        // Act
        var actor = fixture.CreateActor<Description>();
        var expectedName = actor.Get<Description>().Name;

        // Assert
        dict
            .ContainsKey(expectedName)
            .Should()
            .BeTrue();

        dict[expectedName]
            .Id.Should()
            .Be(actor.Id);
    }

    [Fact(DisplayName = "Должен обновлять ключ в словаре при изменении компонента")]
    public void Should_Update_Key_When_Component_Updated()
    {
        // Arrange
        var actor = fixture.CreateActor();
        actor.Add(new Description { Name = "OldName" });

        using var dict = new ActorDictionary<string, Description>(
            fixture.Actors,
            c => c.Name);

        // Act
        // Обновляем компонент, что должно вызвать OnComponentUpdating
        actor.Update(new Description { Name = "NewName" });

        // Assert
        dict
            .ContainsKey("OldName")
            .Should()
            .BeFalse();

        dict
            .ContainsKey("NewName")
            .Should()
            .BeTrue();

        dict["NewName"]
            .Id
            .Should()
            .Be(actor.Id);
    }

    [Fact(DisplayName = "Должен удалять актора из словаря при удалении компонента")]
    public void Should_Remove_Actor_When_Component_Removed()
    {
        // Arrange
        var actor = fixture.CreateActor();
        actor.Add(new Description { Name = "ToRemove" });

        using var dict = new ActorDictionary<string, Description>(
            fixture.Actors,
            c => c.Name);

        // Act
        actor.Remove<Description>();

        // Assert
        dict
            .ContainsKey("ToRemove")
            .Should()
            .BeFalse();

        dict
            .Length
            .Should()
            .Be(0);
    }

    [Fact(DisplayName = "Должен генерировать события при добавлении и удалении")]
    public void Should_Raise_Events_On_Changes()
    {
        // Arrange
        using var dict = new ActorDictionary<string, Description>(
            fixture.Actors,
            c => c.Name);

        var addedRaised = false;
        var removedRaised = false;
        dict.Added += _ => addedRaised = true;
        dict.Removed += _ => removedRaised = true;

        // Act
        var actor = fixture.CreateActor();
        actor.Add(new Description { Name = "EventTest" });
        actor.Remove<Description>();

        // Assert
        addedRaised
            .Should()
            .BeTrue();

        removedRaised
            .Should()
            .BeTrue();
    }

    [Fact(DisplayName = "Должен возвращать ActorRef через TryGetActorRef")]
    public void Should_TryGet_ActorRef()
    {
        // Arrange
        var actor = fixture.CreateActor();
        actor.Add(new Description { Name = "Target" });

        using var dict = new ActorDictionary<string, Description>(
            fixture.Actors,
            c => c.Name);

        // Act
        var found = dict.TryGetActorRef("Target", out var actorRef);
        var notFound = dict.TryGetActorRef("Unknown", out _);

        // Assert
        found.Should().BeTrue();
        actorRef.Id.Should().Be(actor.Id);
        notFound.Should().BeFalse();
    }

    [Fact(DisplayName = "Должен выбрасывать исключение при обращении по несуществующему ключу через индексер")]
    public void Should_Throw_When_Key_Not_Found_In_Indexer()
    {
        // Arrange
        using var dict = new ActorDictionary<string, Description>(
            fixture.Actors,
            c => c.Name);

        // Act
        var action = () => { _ = dict["Missing"]; };

        // Assert
        // Исходя из кода ActorError.KeyNotFound()
        action.Should().Throw<Exception>();
    }

    [Fact(DisplayName = "Должен очищаться при очистке контекста")]
    public void Should_Clear_When_Context_Cleared()
    {
        // Arrange
        var actor = fixture.CreateActor();
        actor.Add(new Description { Name = "ClearMe" });

        using var dict = new ActorDictionary<string, Description>(
            fixture.Actors,
            c => c.Name);

        // Act
        fixture.Actors.Clear();

        // Assert
        dict.Length.Should().Be(0);
        dict.ContainsKey("ClearMe").Should().BeFalse();
    }
}