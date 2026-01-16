using Hexecs.Tests.Mocks;
using Hexecs.Tests.Mocks.ActorComponents;

namespace Hexecs.Tests.Actors;

public sealed class ActorRelationShould(ActorTestFixture fixture) : IClassFixture<ActorTestFixture>
{
    [Fact(DisplayName = "Создавать отношение")]
    public void CreateRelation()
    {
        // arrange

        var actor1 = fixture.CreateActor<Attack>();
        var actor2 = fixture.CreateActor<Attack>();

        // act

        actor1.AddRelation(actor2, new RelationMock());

        // assert

        actor1
            .HasRelation<RelationMock>(actor2)
            .Should()
            .BeTrue();

        actor2
            .HasRelation<RelationMock>(actor1)
            .Should()
            .BeTrue();
    }

    [Fact(DisplayName = "Перечислить отношение")]
    public void EnumerateRelation()
    {
        // arrange

        var expectedRelation = new RelationMock { Value = 123 };

        var actor1 = fixture.CreateActor<Attack>();
        var actor2 = fixture.CreateActor<Attack>();

        actor1.AddRelation(actor2, in expectedRelation);

        // act

        var relationEnumerator = actor1.Relations<RelationMock>();

        // assert

        foreach (var relation in relationEnumerator)
        {
            var actualRelation = relation.Relation;
            actualRelation
                .Should()
                .Be(expectedRelation);
        }
    }

    [Fact(DisplayName = "Получать отношение")]
    public void GetRelation()
    {
        // arrange

        var expectedRelation = new RelationMock { Value = 123 };

        var actor1 = fixture.CreateActor<Attack>();
        var actor2 = fixture.CreateActor<Attack>();

        actor1.AddRelation(actor2, in expectedRelation);

        // act

        var actualRelation = actor1.GetRelation<RelationMock>(actor2);

        // assert

        actualRelation
            .Should()
            .Be(expectedRelation);
    }

    [Fact(DisplayName = "Удалять отношение")]
    public void RemoveRelation()
    {
        // arrange

        var actor1 = fixture.CreateActor<Attack>();
        var actor2 = fixture.CreateActor<Attack>();
        actor1.AddRelation(actor2, new RelationMock());

        // act

        actor1.RemoveRelation<RelationMock>(actor2);

        // assert

        actor1
            .HasRelation<RelationMock>(actor2)
            .Should()
            .BeFalse();

        actor2.HasRelation<RelationMock>(actor1)
            .Should()
            .BeFalse();
    }

    [Fact(DisplayName = "Не удалять несуществующее отношение")]
    public void NotRemoveNonExistentRelation()
    {
        // arrange
        var actor1 = fixture.CreateActor<Attack>();
        var actor2 = fixture.CreateActor<Attack>();

        // act & assert
        actor1.Invoking(a => a.RemoveRelation<RelationMock>(actor2))
            .Should()
            .NotThrow();
    }

    [Fact(DisplayName = "Возвращать false при проверке несуществующего отношения")]
    public void ReturnFalseForNonExistentRelation()
    {
        // arrange
        var actor1 = fixture.CreateActor<Attack>();
        var actor2 = fixture.CreateActor<Attack>();

        // act & assert
        actor1.HasRelation<RelationMock>(actor2)
            .Should()
            .BeFalse();
    }

    [Fact(DisplayName = "Возвращать пустой енумератор когда нет отношений")]
    public void ReturnEmptyEnumeratorWhenNoRelations()
    {
        // arrange
        var actor = fixture.CreateActor<Attack>();

        // act
        var relations = actor.Relations<RelationMock>();

        // assert
        relations.Length.Should().Be(0);
    }

    [Fact(DisplayName = "Перезаписывать существующее отношение")]
    public void OverwriteExistingRelation()
    {
        // arrange
        var actor1 = fixture.CreateActor<Attack>();
        var actor2 = fixture.CreateActor<Attack>();
        var initialRelation = new RelationMock { Value = 100 };
        var newRelation = new RelationMock { Value = 200 };

        actor1.AddRelation(actor2, in initialRelation);

        // act
        ref var existsRelation = ref actor1.GetRelation<RelationMock>(actor2);
        existsRelation = newRelation;

        // assert
        var actualRelation = actor1.GetRelation<RelationMock>(actor2);
        actualRelation.Value.Should().Be(200);
    }

    [Fact(DisplayName = "Поддерживать множественные отношения с разными актёрами")]
    public void SupportMultipleRelationsWithDifferentActors()
    {
        // arrange
        var actor1 = fixture.CreateActor<Attack>();
        var actor2 = fixture.CreateActor<Attack>();
        var actor3 = fixture.CreateActor<Attack>();

        var relation12 = new RelationMock { Value = 100 };
        var relation13 = new RelationMock { Value = 200 };

        // act
        actor1.AddRelation(actor2, in relation12);
        actor1.AddRelation(actor3, in relation13);

        // assert
        actor1.GetRelation<RelationMock>(actor2).Value.Should().Be(100);
        actor1.GetRelation<RelationMock>(actor3).Value.Should().Be(200);
    }

    [Fact(DisplayName = "Выбрасывать ошибку при попытке добавить существующее отношение")]
    public void ThrowWhenAddingExistingRelation()
    {
        // arrange
        var actor1 = fixture.CreateActor<Attack>();
        var actor2 = fixture.CreateActor<Attack>();
        var relation = new RelationMock { Value = 100 };

        actor1.AddRelation(actor2, in relation);

        // act & assert
        actor1.Invoking(a => a.AddRelation(actor2, in relation))
            .Should()
            .Throw<Exception>();
    }

    [Fact(DisplayName = "Выбрасывать ошибку при попытке получить несуществующее отношение")]
    public void ThrowWhenGettingNonExistentRelation()
    {
        // arrange
        var actor1 = fixture.CreateActor<Attack>();
        var actor2 = fixture.CreateActor<Attack>();

        // act & assert
        actor1.Invoking(a => a.GetRelation<RelationMock>(actor2))
            .Should()
            .Throw<Exception>();
    }
}