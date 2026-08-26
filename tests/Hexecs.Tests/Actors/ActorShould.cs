using System.Runtime.CompilerServices;

using Hexecs.Assets;
using Hexecs.Tests.Mocks;
using Hexecs.Tests.Mocks.ActorComponents;
using Hexecs.Tests.Mocks.Assets;

namespace Hexecs.Tests.Actors;

public sealed class ActorShould(ActorTestFixture fixture) : IClassFixture<ActorTestFixture>
{
    [Fact(DisplayName = "Добавить компонент")]
    public void AddComponent()
    {
        // arrange

        Actor actor = fixture.CreateActor();
        var component = fixture.CreateComponent<Attack>();

        // act

        actor.Add(component);

        // assert

        actor
            .Has<Attack>()
            .Should()
            .BeTrue();

        actor
            .Get<Attack>()
            .Should()
            .Be(component);
    }

    [Fact(DisplayName = "Добавить дочерний актор")]
    public void AddChildActor()
    {
        // arrange
        Actor parent = fixture.CreateActor();
        Actor child = fixture.CreateActor();

        // act
        parent.AddChild(child);

        // assert
        parent.Children()
            .ToArray()
            .Should()
            .Contain(c => c.Id == child.Id);
    }

    [Fact(DisplayName = "Добавить отношение между акторами")]
    public void AddRelationBetweenActors()
    {
        // arrange
        Actor actor1 = fixture.CreateActor();
        Actor actor2 = fixture.CreateActor();
        var relation = new RelationMock { Value = 42 };

        // act
        actor1.AddRelation(actor2, relation);

        // assert
        actor1
            .HasRelation<RelationMock>(actor2)
            .Should()
            .BeTrue();

        actor1
            .GetRelation<RelationMock>(actor2)
            .Should()
            .Be(relation);
    }

    [Fact(DisplayName = "Преобразовываться к Actor1")]
    public void AsActor1()
    {
        // arrange

        Actor actor = fixture.CreateActor();
        var component = fixture.CreateComponent<Attack>();
        actor.Add(component);

        // act

        ActorRef<Attack> actor1 = actor.AsRef<Attack>();

        // assert

        actor1
            .Component1
            .Should()
            .Be(component);
    }

    [Fact(DisplayName = "Существовать")]
    public void BeAlive()
    {
        // arrange

        Actor actor = fixture.CreateActor();

        // act, assert

        actor
            .Alive
            .Should()
            .BeTrue();
    }

    [Fact(DisplayName = "Должен быть клонирован")]
    public void BeClone()
    {
        // arrange

        var attack = fixture.CreateComponent<Attack>();
        var defence = fixture.CreateComponent<Defence>();
        Actor actor = fixture.CreateActor<Attack, Defence>(component1: attack, component2: defence);
        ActorContext context = fixture.Actors;

        // act

        Actor clone = context.Clone(actor.Id);

        // assert

        clone.Get<Attack>()
            .Should()
            .BeEquivalentTo(attack);

        clone.Get<Defence>()
            .Should()
            .BeEquivalentTo(defence);
    }

    [Fact(DisplayName = "Должен быть имеющим нужный компонент")]
    public void BeIs()
    {
        // arrange

        Actor expected = fixture.CreateActor<Defence>();

        // act

        expected
            .IsRef<Defence>(out ActorRef<Defence> actual)
            .Should()
            .BeTrue();

        // assert

        expected.Get<Defence>()
            .Should()
            .Be(actual.Component1);
    }

    [Fact(DisplayName = "Проверить пустой актор")]
    public void CheckEmptyActor()
    {
        // arrange
        var emptyActor = Actor.Empty;

        // act, assert
        emptyActor.IsEmpty.Should().BeTrue();
        emptyActor.Alive.Should().BeFalse();
    }

    [Fact(DisplayName = "Проверить наличие отношения между акторами")]
    public void CheckRelationExistsBetweenActors()
    {
        // arrange
        Actor actor1 = fixture.CreateActor();
        Actor actor2 = fixture.CreateActor();
        var relation = new RelationMock { Value = 42 };
        actor1.AddRelation(actor2, relation);

        // act, assert
        actor1.HasRelation<RelationMock>(actor2).Should().BeTrue();
        actor1.HasRelation<RelationMock>(fixture.CreateActor()).Should().BeFalse();
    }

    [Fact(DisplayName = "Успешно проверить на наличие нескольких компонентов")]
    public void CheckMultipleComponentsExistence()
    {
        // arrange
        var attack = fixture.CreateComponent<Attack>();
        var defence = fixture.CreateComponent<Defence>();
        Actor actor = fixture.CreateActor<Attack, Defence>(component1: attack, component2: defence);

        // act, assert
        actor
            .Has<Attack>()
            .Should()
            .BeTrue();

        actor
            .Has<Defence>()
            .Should()
            .BeTrue();
    }

    [Fact(DisplayName = "Проверить отсутствие актора через IsRef")]
    public void CheckActorDoesNotExistViaIsRef()
    {
        // arrange
        Actor actor = fixture.CreateActor<Defence>();

        // act
        bool result = actor.IsRef<Attack>(out ActorRef<Attack> actorRef);

        // assert
        result.Should().BeFalse();

        actorRef.IsEmpty
            .Should()
            .BeTrue();
    }

    [Fact(DisplayName = "Преобразоваться к ActorRef")]
    public void ConvertToActorRef()
    {
        // arrange
        var component = fixture.CreateComponent<Attack>();
        Actor actor = fixture.CreateActor<Attack>(component1: component);

        // act
        ActorRef<Attack> actorRef = actor.AsRef<Attack>();

        // assert
        actorRef.Component1.Should().Be(component);
    }

    [Fact(DisplayName = "Не иметь не добавленный компонент")]
    public void DoesntHaveNotExistsComponent()
    {
        // arrange

        Actor actor = fixture.CreateActor<Defence>();

        // act, assert

        actor
            .Has<Attack>()
            .Should()
            .BeFalse();
    }

    [Fact(DisplayName = "Нет ошибки при попытке получить не существующий компонент")]
    public void DoesntThrowIfTryGetDoesNotExistComponent()
    {
        // arrange

        Actor actor = fixture.CreateActor();

        // act

        ref Attack componentRef = ref actor.TryGetRef<Attack>();

        // assert

        Unsafe
            .IsNullRef(ref componentRef)
            .Should()
            .BeTrue();
    }

    [Fact(DisplayName = "Проверить равенство акторов")]
    public void Equality()
    {
        // arrange
        Actor actor1 = fixture.CreateActor();
        Actor actor2 = actor1;
        Actor actor3 = fixture.CreateActor(); // Актор с другим ID

        // act, assert
        actor1.Equals(actor1).Should().BeTrue(); // Сам с собой
        actor1.Equals(actor2).Should().BeTrue(); // С тем же ID
        actor1.Equals(actor3).Should().BeFalse(); // С другим ID
        (actor1 == actor2).Should().BeTrue(); // Оператор ==
        (actor1 != actor3).Should().BeTrue(); // Оператор !=
    }

    [Fact(DisplayName = "Получить привязанный ассет")]
    public void GetBoundAsset()
    {
        // arrange

        AssetRef<UnitAsset> asset = fixture.Assets.GetAssetRef<UnitAsset>();
        Actor actor = fixture.Actors.BuildActor(asset);

        // act
        Asset retrievedAsset = actor.GetAsset();

        // assert
        retrievedAsset
            .AsRef<UnitAsset>()
            .Equals(asset)
            .Should()
            .BeTrue();
    }

    [Fact(DisplayName = "Получить ссылку на компонент")]
    public void GetComponentRef()
    {
        // arrange
        var attack = fixture.CreateComponent<Attack>();
        Actor actor = fixture.CreateActor<Attack>(component1: attack);

        // act
        ref Attack componentRef = ref actor.Get<Attack>();

        // assert
        Unsafe
            .AreSame(ref componentRef, ref actor.Get<Attack>())
            .Should()
            .BeTrue();
    }

    [Fact(DisplayName = "Получить родителя актора")]
    public void GetParentActor()
    {
        // arrange
        Actor parent = fixture.CreateActor();
        Actor child = fixture.CreateActor();
        parent.AddChild(child);

        // act
        bool hasParent = child.TryGetParent(out Actor retrievedParent);

        // assert
        hasParent.Should().BeTrue();
        retrievedParent.Should().Be(parent);
    }

    [Fact(DisplayName = "Получить отношение между акторами")]
    public void GetRelationBetweenActors()
    {
        // arrange
        Actor actor1 = fixture.CreateActor();
        Actor actor2 = fixture.CreateActor();
        var relation = new RelationMock { Value = 42 };
        actor1.AddRelation(actor2, relation);

        // act
        ref RelationMock retrievedRelation = ref actor1.GetRelation<RelationMock>(actor2);

        // assert
        retrievedRelation.Should().Be(relation);
    }

    [Fact(DisplayName = "Проверить неявное преобразование актора в bool")]
    public void ImplicitCastToBool()
    {
        // arrange
        Actor actor = fixture.CreateActor();
        var emptyActor = Actor.Empty;

        // act, assert
        ((bool)actor).Should().BeTrue();
        ((bool)emptyActor).Should().BeFalse();
    }

    [Fact(DisplayName = "Проверить неявное преобразование актора в ActorId")]
    public void ImplicitCastToActorId()
    {
        // arrange
        Actor actor = fixture.CreateActor();

        // act
        ActorId actorId = actor;

        // assert
        actorId
            .Should()
            .Be(actor.Id);
    }

    [Fact(DisplayName = "Удалить отношение между акторами")]
    public void RemoveRelationBetweenActors()
    {
        // arrange
        Actor actor1 = fixture.CreateActor();
        Actor actor2 = fixture.CreateActor();
        var relation = new RelationMock { Value = 42 };
        actor1.AddRelation(actor2, relation);

        // act
        bool result = actor1.RemoveRelation<RelationMock>(actor2, out RelationMock removedRelation);

        // assert
        result.Should().BeTrue();
        removedRelation.Should().Be(relation);
        actor1.HasRelation<RelationMock>(actor2).Should().BeFalse();
    }

    [Fact(DisplayName = "Не должен существовать")]
    public void NotBeAlive()
    {
        // arrange

        Actor actor = fixture.CreateActor();

        // act

        actor.Destroy();

        // assert

        actor
            .Alive
            .Should()
            .BeFalse();
    }

    [Fact(DisplayName = "Не должен быть имеющим нужный компонент")]
    public void NotBeIs()
    {
        // arrange

        Actor expected = fixture.CreateActor<Defence>();

        // act

        expected
            .IsRef<Attack>(out ActorRef<Attack> actual)
            .Should()
            .BeFalse();

        // assert

        expected
            .Equals(actual)
            .Should()
            .BeFalse();
    }

    [Fact(DisplayName = "Не удалить несуществующий компонент")]
    public void NotRemoveNonExistingComponent()
    {
        // arrange
        Actor actor = fixture.CreateActor<Attack>();

        // act
        bool result = actor.Remove<Defence>(out Defence removed);

        // assert
        result
            .Should()
            .BeFalse();

        removed
            .Should()
            .BeEquivalentTo(default(Defence));
    }

    [Fact(DisplayName = "Неуспешно пробовать получить несуществующий компонент")]
    public void NotTryGetNonExistingComponent()
    {
        // arrange
        Actor actor = fixture.CreateActor<Attack>();

        // act
        ref Defence result = ref actor.TryGetRef<Defence>();

        // assert
        Unsafe
            .IsNullRef(ref result)
            .Should()
            .BeTrue();
    }

    [Fact(DisplayName = "Удалить компонент")]
    public void RemoveComponent()
    {
        // arrange

        var component = fixture.CreateComponent<Attack>();
        Actor actor = fixture.CreateActor<Attack>(component1: component);

        // act

        actor
            .Remove<Attack>(out Attack removed)
            .Should()
            .BeTrue();

        // assert

        actor
            .Has<Attack>()
            .Should()
            .BeFalse();

        removed
            .Should()
            .Be(component);
    }

    [Fact(DisplayName = "Удалить дочерний актор")]
    public void RemoveChildActor()
    {
        // arrange
        Actor parent = fixture.CreateActor();
        Actor child = fixture.CreateActor();
        parent.AddChild(child);

        // act
        bool result = parent.RemoveChild(child);

        // assert
        result.Should().BeTrue();
        parent
            .Children()
            .ToArray()
            .Should()
            .NotContain(c => c.Id == child.Id);
    }

    [Fact(DisplayName = "Ошибка при получении не существующего компонента")]
    public void ThrowIfGetDoesNotExistComponent()
    {
        // arrange

        Actor actor = fixture.CreateActor<Defence>();

        // act, assert

        actor
            .Invoking(a => a.Get<Attack>())
            .Should()
            .Throw<Exception>();
    }

    [Fact(DisplayName = "Ошибка при добавлении существующего компонента")]
    public void ThrowIfAddingComponentExists()
    {
        // arrange

        Actor actor = fixture.CreateActor<Defence>();

        // act, assert

        actor
            .Invoking(a => a.Add(fixture.CreateComponent<Defence>()))
            .Should()
            .Throw<Exception>();
    }

    [Fact(DisplayName = "Попытаться добавить компонент")]
    public void TryAddComponent()
    {
        // arrange
        Actor actor = fixture.CreateActor();
        var component = fixture.CreateComponent<Attack>();

        // act
        bool result = actor.TryAdd(component);

        // assert
        result.Should().BeTrue();
        actor.Has<Attack>().Should().BeTrue();
        actor.Get<Attack>().Should().Be(component);
    }

    [Fact(DisplayName = "Попытаться получить привязанный ассет")]
    public void TryGetBoundAsset()
    {
        // arrange

        AssetRef<UnitAsset> asset = fixture.Assets.GetAssetRef<UnitAsset>();
        Actor actor = fixture.Actors.BuildActor(asset);

        // act
        bool result = actor.TryGetAsset(out Asset actualAsset);

        // assert

        result
            .Should()
            .BeTrue();

        actualAsset
            .AsRef<UnitAsset>()
            .Equals(asset)
            .Should()
            .BeTrue();
    }

    [Fact(DisplayName = "Успешно пробовать получить существующий компонент")]
    public void TryGetExistingComponent()
    {
        // arrange
        var attack = fixture.CreateComponent<Attack>();
        Actor actor = fixture.CreateActor<Attack>(component1: attack);

        // act
        var result = actor.TryGetRef<Attack>();

        // assert
        result
            .Should()
            .Be(attack);
    }

    [Fact(DisplayName = "Обновить компонент актора")]
    public void UpdateActorComponent()
    {
        // arrange
        var initialComponent = fixture.CreateComponent<Attack>();
        Actor actor = fixture.CreateActor<Attack>(component1: initialComponent);
        var updatedComponent = fixture.CreateComponent<Attack>();

        // act
        bool result = actor.Update(updatedComponent);

        // assert
        result.Should().BeTrue();
        actor
            .Get<Attack>()
            .Should()
            .Be(updatedComponent);
    }
}