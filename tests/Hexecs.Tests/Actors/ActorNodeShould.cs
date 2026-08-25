namespace Hexecs.Tests.Actors;

public sealed class ActorNodeShould(ActorTestFixture fixture) : IClassFixture<ActorTestFixture>
{
    [Fact(DisplayName = "Дочерний элемент должен существовать")]
    public void HasChild()
    {
        // Arrange

        var expectedChild = fixture.CreateActor();
        var parent = fixture.CreateActor();

        // Act

        parent.AddChild(expectedChild);

        // Assert

        parent
            .Children()
            .ToArray()
            .Should()
            .Contain(expectedChild);
    }

    [Fact(DisplayName = "Дочерние элементы должны существовать")]
    public void HasAllChild()
    {
        // Arrange

        var expectedChild1 = fixture.CreateActor();
        var expectedChild3 = fixture.CreateActor();
        var expectedChild2 = fixture.CreateActor();
        var parent = fixture.CreateActor();

        // Act

        parent.AddChild(expectedChild1);
        parent.AddChild(expectedChild2);
        parent.AddChild(expectedChild3);

        // Assert

        var children = parent.Children();
        children
            .Length
            .Should().Be(3);

        children
            .ToArray()
            .Should()
            .Contain([expectedChild1, expectedChild2, expectedChild3]);
    }

    [Fact(DisplayName = "Родитель должен существовать")]
    public void HasParent()
    {
        // Arrange

        var child = fixture.CreateActor();
        var expectedParent = fixture.CreateActor();

        // Act

        expectedParent.AddChild(child);

        // Assert

        child
            .TryGetParent(out var actualParent)
            .Should()
            .BeTrue();

        actualParent
            .Should()
            .Be(expectedParent);
    }

    [Fact(DisplayName = "Дочерний элемент не должен иметь родителя после уничтожения родителя")]
    public void RemoveChildAfterDestroyParent()
    {
        // Arrange

        var child = fixture.CreateActor();
        var parent = fixture.CreateActor();

        parent.AddChild(child);

        // Act

        parent.Destroy();

        // Assert

        child
            .TryGetParent(out var actualParent)
            .Should()
            .BeFalse();

        actualParent
            .Should()
            .BeEquivalentTo(Actor.Empty);
    }

    [Fact(DisplayName = "Дочерний элемент должен быть удалён из родителя после уничтожения дочернего элемента")]
    public void RemoveChildAfterDestroyChild()
    {
        // Arrange

        var child1 = fixture.CreateActor();
        var child2 = fixture.CreateActor();
        var child3 = fixture.CreateActor();
        var parent = fixture.CreateActor();

        parent.AddChild(child1);
        parent.AddChild(child2);
        parent.AddChild(child3);

        // Act

        child2.Destroy();

        // Assert

        var children = parent.Children();
        children
            .Length
            .Should().Be(2);

        children
            .ToArray()
            .Should()
            .Contain([child1, child3]);

        children
            .ToArray()
            .Should()
            .NotContain(child2);
    }

    [Fact(DisplayName = "Дочерний элемент должен сменить родителя при добавлении к новому")]
    public void ChangeParentCorrectly()
    {
        // Arrange
        var child = fixture.CreateActor();
        var parent1 = fixture.CreateActor();
        var parent2 = fixture.CreateActor();

        // Act
        parent1.AddChild(child);
        parent2.AddChild(child); // Должен автоматически удалиться из parent1

        // Assert
        parent1.Children().Length.Should().Be(0);
        parent2.Children().Length.Should().Be(1);

        child.TryGetParent(out var actualParent).Should().BeTrue();
        actualParent.Should().Be(parent2);
    }

    [Fact(DisplayName = "Удаление первого ребенка должно корректно обновлять FirstChildId")]
    public void RemoveFirstChildCorrectly()
    {
        // Arrange
        var parent = fixture.CreateActor();
        var child1 = fixture.CreateActor();
        var child2 = fixture.CreateActor();
        parent.AddChild(child1); // child1 станет NextSibling для child2
        parent.AddChild(child2); // child2 теперь FirstChildId

        // Act
        child2.Destroy();

        // Assert
        parent.Children().Length.Should().Be(1);
        parent.Children().ToArray().Should().ContainSingle().Which.Should().Be(child1);
    }

    [Fact(DisplayName = "Удаление среднего ребенка должно корректно связывать соседей")]
    public void RemoveMiddleChildCorrectly()
    {
        // Arrange
        var parent = fixture.CreateActor();
        var child1 = fixture.CreateActor();
        var child2 = fixture.CreateActor();
        var child3 = fixture.CreateActor();

        // Порядок в списке (LIFO): child3 -> child2 -> child1
        parent.AddChild(child1);
        parent.AddChild(child2);
        parent.AddChild(child3);

        // Act
        child2.Destroy(); // Удаляем средний

        // Assert
        var children = parent.Children().ToArray();
        children.Length.Should().Be(2);
        children[0].Should().Be(child3);
        children[1].Should().Be(child1);
    }

    [Fact(DisplayName = "Проверка метода HasChild")]
    public void HasChildMethodWorks()
    {
        // Arrange
        var parent = fixture.CreateActor();
        var child = fixture.CreateActor();
        var stranger = fixture.CreateActor();

        // Act
        parent.AddChild(child);

        // Assert
        parent.HasChild(child).Should().BeTrue();
        parent.HasChild(stranger).Should().BeFalse();
    }

    [Fact(DisplayName = "Родитель не может добавить самого себя в дети")]
    public void ParentCannotAddItselfAsChild()
    {
        // Arrange
        var actor = fixture.CreateActor();

        // Act
        actor.AddChild(actor);

        // Assert
        actor.Children().Length.Should().Be(0);
        actor.TryGetParent(out _).Should().BeFalse();
    }
}