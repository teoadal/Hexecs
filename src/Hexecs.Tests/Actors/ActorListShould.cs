using Hexecs.Tests.Mocks;

namespace Hexecs.Tests.Actors;

public sealed class ActorListShould(ActorTestFixture fixture) : IClassFixture<ActorTestFixture>
{
    [Fact(DisplayName = "Должен успешно добавлять акторов и увеличивать размер списка")]
    public void Should_Add_Actors_And_Increase_Length()
    {
        // Arrange
        using var list = new ActorList<Defence>(fixture.Actors);
        var actor = fixture.CreateActor<Defence>();

        // Act
        list.Add(actor);

        // Assert
        list.Length.Should().Be(1);
        list.Contains(actor).Should().BeTrue();
    }

    [Fact(DisplayName = "Должен возвращать false при попытке удалить несуществующего актора")]
    public void Should_Return_False_When_Removing_Non_Existent_Actor()
    {
        // Arrange
        using var list = new ActorList<Defence>(fixture.Actors);
        uint fakeId = 999;

        // Act
        var result = list.Remove(fakeId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact(DisplayName = "Должен очищать список и освобождать ресурсы при вызове Clear")]
    public void Should_Clear_All_Actors()
    {
        // Arrange
        using var list = new ActorList<Defence>(fixture.Actors);
        list.Add(fixture.CreateActor<Defence>());
        list.Add(fixture.CreateActor<Defence>());

        // Act
        list.Clear();

        // Assert
        list.Length.Should().Be(0);
        list.GetEnumerator().MoveNext().Should().BeFalse();
    }

    [Fact(DisplayName = "Должен автоматически удалять актора из списка, если его компонент удален из пула")]
    public void Should_Automatically_Remove_Actor_When_Component_Is_Removed()
    {
        // Arrange
        using var list = new ActorList<Defence>(fixture.Actors);
        var actor = fixture.CreateActor<Defence>();
        list.Add(actor);

        // Act
        actor.Remove<Defence>(); // Это вызывает событие Removing в пуле

        // Assert
        list.Contains(actor).Should().BeFalse();
        list.Length.Should().Be(0);
    }

    [Fact(DisplayName = "Должен корректно итерироваться по добавленным акторам")]
    public void Should_Enumerate_Added_Actors()
    {
        // Arrange
        using var list = new ActorList<Defence>(fixture.Actors);
        var actor1 = fixture.CreateActor<Defence>();
        var actor2 = fixture.CreateActor<Defence>();
        list.Add(actor1);
        list.Add(actor2);

        // Act
        var resultIds = new List<uint>();
        foreach (var actor in list)
        {
            resultIds.Add(actor.Id);
        }

        // Assert
        resultIds
            .Should()
            .HaveCount(2)
            .And.Contain([actor1.Id, actor2.Id]);
    }

    [Fact(DisplayName = "Должен выбрасывать исключение при попытке добавления в Dispose объект")]
    public void Should_Throw_When_Adding_To_Disposed_List()
    {
        // Arrange
        var list = new ActorList<Defence>(fixture.Actors);
        list.Dispose();
        var actor = fixture.CreateActor<Defence>();

        // Act
        var action = () => list.Add(actor);

        // Assert
        action.Should().Throw<Exception>()
            .WithMessage("*disposed*");
    }

    [Theory(DisplayName = "Должен поддерживать добавление акторов через разные типы ссылок")]
    [InlineData(true)]
    [InlineData(false)]
    public void Should_Add_Using_Different_References(bool useActorRef)
    {
        // Arrange
        using var list = new ActorList<Defence>(fixture.Actors);
        var actor = fixture.CreateActor<Defence>();

        // Act
        if (useActorRef)
            list.Add(actor.AsRef());
        else
            list.Add(actor);

        // Assert
        list.Contains(actor.Id).Should().BeTrue();
    }
}