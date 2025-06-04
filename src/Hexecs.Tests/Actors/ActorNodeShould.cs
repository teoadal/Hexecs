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

    [Fact(DisplayName = "Родитель должен быть удалён после уничтожения родителя")]
    public void RemoveParentAfterDestroyParent()
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
    
    [Fact(DisplayName = "Дочерний элемент должен быть удалён после уничтожения")]
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
}