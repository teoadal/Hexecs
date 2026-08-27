namespace Hexecs.Tests.Actors;

public sealed class ActorNodeShould(ActorTestFixture fixture) : IClassFixture<ActorTestFixture>
{
    [Fact(DisplayName = "Дочерний элемент должен существовать")]
    public void HasChild()
    {
        // Arrange

        Actor expectedChild = fixture.CreateActor();
        Actor parent = fixture.CreateActor();

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

        Actor expectedChild1 = fixture.CreateActor();
        Actor expectedChild3 = fixture.CreateActor();
        Actor expectedChild2 = fixture.CreateActor();
        Actor parent = fixture.CreateActor();

        // Act

        parent.AddChild(expectedChild1);
        parent.AddChild(expectedChild2);
        parent.AddChild(expectedChild3);

        // Assert

        ActorContext.ChildrenEnumerator children = parent.Children();
        children
            .Length
            .Should()
            .Be(3);

        children
            .ToArray()
            .Should()
            .Contain([expectedChild1, expectedChild2, expectedChild3]);
    }

    [Fact(DisplayName = "Родитель должен существовать")]
    public void HasParent()
    {
        // Arrange

        Actor child = fixture.CreateActor();
        Actor expectedParent = fixture.CreateActor();

        // Act

        expectedParent.AddChild(child);

        // Assert

        child
            .TryGetParent(out Actor actualParent)
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

        Actor child = fixture.CreateActor();
        Actor parent = fixture.CreateActor();

        parent.AddChild(child);

        // Act

        parent.Destroy();

        // Assert

        child
            .TryGetParent(out Actor actualParent)
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

        Actor child1 = fixture.CreateActor();
        Actor child2 = fixture.CreateActor();
        Actor child3 = fixture.CreateActor();
        Actor parent = fixture.CreateActor();

        parent.AddChild(child1);
        parent.AddChild(child2);
        parent.AddChild(child3);

        // Act

        child2.Destroy();

        // Assert

        ActorContext.ChildrenEnumerator children = parent.Children();
        children
            .Length
            .Should()
            .Be(2);

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
        Actor child = fixture.CreateActor();
        Actor parent1 = fixture.CreateActor();
        Actor parent2 = fixture.CreateActor();

        // Act
        parent1.AddChild(child);
        parent2.AddChild(child); // Должен автоматически удалиться из parent1

        // Assert
        parent1.Children().Length.Should().Be(0);
        parent2.Children().Length.Should().Be(1);

        child.TryGetParent(out Actor actualParent).Should().BeTrue();
        actualParent.Should().Be(parent2);
    }

    [Fact(DisplayName = "Удаление первого ребенка должно корректно обновлять FirstChildId")]
    public void RemoveFirstChildCorrectly()
    {
        // Arrange
        Actor parent = fixture.CreateActor();
        Actor child1 = fixture.CreateActor();
        Actor child2 = fixture.CreateActor();
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
        Actor parent = fixture.CreateActor();
        Actor child1 = fixture.CreateActor();
        Actor child2 = fixture.CreateActor();
        Actor child3 = fixture.CreateActor();

        // Порядок в списке (LIFO): child3 -> child2 -> child1
        parent.AddChild(child1);
        parent.AddChild(child2);
        parent.AddChild(child3);

        // Act
        child2.Destroy(); // Удаляем средний

        // Assert
        Actor[] children = parent.Children().ToArray();
        children.Length.Should().Be(2);
        children[0].Should().Be(child3);
        children[1].Should().Be(child1);
    }

    [Fact(DisplayName = "Проверка метода HasChild")]
    public void HasChildMethodWorks()
    {
        // Arrange
        Actor parent = fixture.CreateActor();
        Actor child = fixture.CreateActor();
        Actor stranger = fixture.CreateActor();

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
        Actor actor = fixture.CreateActor();

        // Act
        actor.AddChild(actor);

        // Assert
        actor.Children().Length.Should().Be(0);
        actor.TryGetParent(out _).Should().BeFalse();
    }
}
