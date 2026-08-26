using System.Runtime.CompilerServices;
using Hexecs.Assets;
using Hexecs.Tests.Mocks.ActorComponents;
using Hexecs.Tests.Mocks.Assets;

namespace Hexecs.Tests.Actors;

public sealed class ActorContextShould(ActorTestFixture fixture) : IClassFixture<ActorTestFixture>
{
    [Fact(DisplayName = "Создать актёра по ассету")]
    public void BuildActor()
    {
        // arrange

        AssetRef<UnitAsset> asset = fixture.Assets.GetAssetRef<UnitAsset>();

        // act

        Actor actor = fixture.Actors.BuildActor(asset);

        // assert

        actor
            .GetAsset()
            .Should()
            .Be((Asset)asset);

        actor
            .Get<Attack>().Value
            .Should()
            .Be(asset.Component1.Attack);

        actor
            .Get<Defence>().Value
            .Should()
            .Be(asset.Component1.Defence);
    }

    [Fact(DisplayName = "Создать актёра с заданным Id")]
    public void CreateActorWithExpectedId()
    {
        // arrange
        const uint expectedId = 100_234;

        // act
        Actor actor = fixture.Actors.CreateActor(expectedId);

        // assert
        actor.Id.Value.Should().Be(expectedId);
        fixture.Actors.ActorAlive(new ActorId(expectedId)).Should().BeTrue();
    }

    [Fact(DisplayName = "Проверять существование актёра")]
    public void CheckIfActorIsAlive()
    {
        // arrange
        Actor actor = fixture.Actors.CreateActor();
        const uint nonExistentId = 1_009_999;

        // act
        bool actorExists = fixture.Actors.ActorAlive(actor.Id);
        bool nonExistentActorExists = fixture.Actors.ActorAlive(new ActorId(nonExistentId));

        // assert
        actorExists.Should().BeTrue();
        nonExistentActorExists.Should().BeFalse();
    }

    [Fact(DisplayName = "Удалять актёра по Id")]
    public void DestroyActorById()
    {
        // arrange
        Actor actor = fixture.Actors.CreateActor();

        // act
        bool result = fixture.Actors.DestroyActor(actor.Id);

        // assert
        result.Should().BeTrue();
        fixture.Actors.ActorAlive(actor.Id).Should().BeFalse();
    }

    [Fact(DisplayName = "Получать актёра по Id")]
    public void GetActorById()
    {
        // arrange
        Actor createdActor = fixture.Actors.CreateActor();

        // act
        Actor retrievedActor = fixture.Actors.GetActor(createdActor.Id);

        // assert
        retrievedActor.Id.Should().Be(createdActor.Id);
    }

    [Fact(DisplayName = "Создавать клон актёра")]
    public void CloneActor()
    {
        // arrange
        AssetRef<UnitAsset> asset = fixture.Assets.GetAssetRef<UnitAsset>();
        Actor originalActor = fixture.Actors.BuildActor(asset);

        // act
        Actor clonedActor = fixture.Actors.Clone(originalActor.Id);

        // assert
        clonedActor.Id.Should().NotBe(originalActor.Id);
        clonedActor.GetAsset().Should().Be(originalActor.GetAsset());
        clonedActor.Get<Attack>().Value.Should().Be(originalActor.Get<Attack>().Value);
        clonedActor.Get<Defence>().Value.Should().Be(originalActor.Get<Defence>().Value);
    }

    [Fact(DisplayName = "Очищать все записи актёров")]
    public void ClearAllActors()
    {
        // arrange
        Actor actor1 = fixture.Actors.CreateActor();
        Actor actor2 = fixture.Actors.CreateActor();
        bool initialActor1Exists = fixture.Actors.ActorAlive(actor1.Id);
        bool initialActor2Exists = fixture.Actors.ActorAlive(actor2.Id);

        // act
        fixture.Actors.Clear();

        // assert
        initialActor1Exists.Should().BeTrue();
        initialActor2Exists.Should().BeTrue();
        fixture.Actors.ActorAlive(actor1.Id).Should().BeFalse();
        fixture.Actors.ActorAlive(actor2.Id).Should().BeFalse();
    }

    [Theory(DisplayName = "Пытаться получить актёра с указанным компонентом")]
    [AutoData]
    public void TryGetActorWithComponent(int attackValue, int defenceValue)
    {
        // arrange
        ActorContext actors = fixture.Actors;

        Actor actor = actors.CreateActor();
        actor.Add(new Attack { Value = attackValue });
        actor.Add(new Defence { Value = defenceValue });

        // act
        bool hasAttack = actors.TryGetActorRef<Attack>(actor.Id, out ActorRef<Attack> actorWithAttack);
        bool hasDefence = actors.TryGetActorRef<Defence>(actor.Id, out ActorRef<Defence> actorWithDefence);
        bool hasNonExistentComponent = actors.TryGetActorRef<NonExistentComponent>(actor.Id, out _);

        // assert
        hasAttack.Should().BeTrue();
        actorWithAttack.Id.Should().Be(actor.Id);
        actorWithAttack.Get<Attack>().Value.Should().Be(attackValue);

        hasDefence.Should().BeTrue();
        actorWithDefence.Id.Should().Be(actor.Id);
        actorWithDefence.Get<Defence>().Value.Should().Be(defenceValue);

        hasNonExistentComponent.Should().BeFalse();
    }

    [Fact(DisplayName = "Получать строковое описание актёра")]
    public void GetActorDescription()
    {
        // arrange
        Actor actor = fixture.Actors.CreateActor();
        actor.Add(new Attack { Value = 10 });
        actor.Add(new Defence { Value = 20 });

        // act
        string description = fixture.Actors.GetDescription(actor.Id);

        // assert
        description.Should().Contain(actor.Id.Value.ToString());
        description.Should().Contain(nameof(Attack));
        description.Should().Contain(nameof(Defence));
    }

    [Fact(DisplayName = "Привязывать и получать ассет к актёру")]
    public void BindAndGetAsset()
    {
        // arrange
        AssetRef<UnitAsset> asset = fixture.Assets.GetAssetRef<UnitAsset>();
        Actor actor = fixture.Actors.CreateActor();

        // act
        fixture.Actors.SetBoundAsset(actor.Id, asset);
        Asset boundAsset = fixture.Actors.GetBoundAsset(actor.Id);

        // assert
        boundAsset.Should().NotBeNull();
        boundAsset.Id.Should().Be(asset.Id);
    }

    [Fact(DisplayName = "Добавлять компонент к актёру")]
    public void AddComponentToActor()
    {
        // arrange
        Actor actor = fixture.Actors.CreateActor();
        var attack = new Attack { Value = 10 };

        // act
        fixture.Actors.AddComponent(actor.Id, attack);

        // assert
        fixture.Actors.HasComponent<Attack>(actor.Id).Should().BeTrue();
        fixture.Actors.GetComponent<Attack>(actor.Id).Value.Should().Be(10);
    }

    [Fact(DisplayName = "Клонировать компонент от одного актёра к другому")]
    public void CloneComponentFromOneActorToAnother()
    {
        // arrange
        Actor actor1 = fixture.Actors.CreateActor();
        Actor actor2 = fixture.Actors.CreateActor();
        var attack = new Attack { Value = 15 };
        fixture.Actors.AddComponent(actor1.Id, attack);

        // act
        ref Attack clonedComponent = ref fixture.Actors.CloneComponent<Attack>(actor1.Id, actor2.Id);

        // assert
        fixture.Actors.HasComponent<Attack>(actor2.Id).Should().BeTrue();
        fixture.Actors.GetComponent<Attack>(actor2.Id).Value.Should().Be(15);
        clonedComponent.Value.Should().Be(15);
    }

    [Fact(DisplayName = "Перечислять компоненты актёра")]
    public void EnumerateActorComponents()
    {
        // arrange
        Actor actor = fixture.Actors.CreateActor();
        fixture.Actors.AddComponent(actor.Id, new Attack { Value = 10 });
        fixture.Actors.AddComponent(actor.Id, new Defence { Value = 20 });

        // act
        ActorContext.ComponentEnumerator components = fixture.Actors.Components(actor.Id);
        var componentTypes = new List<Type>();

        foreach (IActorComponent component in components)
        {
            componentTypes.Add(component.GetType());
        }

        // assert
        componentTypes.Should().Contain(typeof(Attack));
        componentTypes.Should().Contain(typeof(Defence));
    }

    [Fact(DisplayName = "Получать компонент актёра")]
    public void GetActorComponent()
    {
        // arrange
        Actor actor = fixture.Actors.CreateActor();
        fixture.Actors.AddComponent(actor.Id, new Attack { Value = 25 });

        // act
        ref Attack component = ref fixture.Actors.GetComponent<Attack>(actor.Id);

        // assert
        component.Value.Should().Be(25);
    }

    [Fact(DisplayName = "Добавлять компонент, если он не существует")]
    public void AddComponentIfNotExists()
    {
        // arrange
        Actor actor = fixture.Actors.CreateActor();

        // act
        ref Attack component = ref fixture.Actors.GetOrAddComponent(actor.Id, _ => new Attack { Value = 30 });

        // assert
        component.Value.Should().Be(30);
        fixture.Actors.HasComponent<Attack>(actor.Id).Should().BeTrue();
    }

    [Fact(DisplayName = "Получать существующий компонент при вызове GetOrAddComponent")]
    public void GetExistingComponentWithGetOrAdd()
    {
        // arrange
        Actor actor = fixture.Actors.CreateActor();
        fixture.Actors.AddComponent(actor.Id, new Attack { Value = 40 });

        // act
        ref Attack component = ref fixture.Actors.GetOrAddComponent(actor.Id, _ => new Attack { Value = 50 });

        // assert
        component.Value.Should().Be(40); // должно вернуть существующее значение
        fixture.Actors.GetComponent<Attack>(actor.Id).Value.Should().Be(40);
    }

    [Fact(DisplayName = "Проверять наличие компонента у актёра")]
    public void CheckIfActorHasComponent()
    {
        // arrange
        Actor actor = fixture.Actors.CreateActor();
        fixture.Actors.AddComponent(actor.Id, new Attack { Value = 10 });

        // act
        bool hasAttack = fixture.Actors.HasComponent<Attack>(actor.Id);
        bool hasDefence = fixture.Actors.HasComponent<Defence>(actor.Id);

        // assert
        hasAttack.Should().BeTrue();
        hasDefence.Should().BeFalse();
    }

    [Fact(DisplayName = "Вызывать обработчик при добавлении компонента")]
    public void CallHandlerWhenComponentAdded()
    {
        // arrange
        Actor actor = fixture.Actors.CreateActor();
        uint? addedActorId = null;

        fixture.Actors.OnComponentAdded<Attack>(id => addedActorId = id.Value);

        // act
        fixture.Actors.AddComponent(actor.Id, new Attack { Value = 10 });

        // assert
        addedActorId.Should().Be(actor.Id.Value);
    }

    [Fact(DisplayName = "Вызывать обработчик со значением при добавлении компонента")]
    public void CallHandlerWithValueWhenComponentAdded()
    {
        // arrange
        Actor actor = fixture.Actors.CreateActor();
        uint? addedActorId = null;
        Attack? addedComponent = null;

        fixture.Actors.OnComponentAdded((ActorId id, ref Attack component) =>
        {
            addedActorId = id.Value;
            addedComponent = component;
        });

        // act
        fixture.Actors.AddComponent(actor.Id, new Attack { Value = 15 });

        // assert
        addedActorId.Should().Be(actor.Id.Value);
        addedComponent.Should().NotBeNull();
        addedComponent?.Value.Should().Be(15);
    }

    [Fact(DisplayName = "Вызывать обработчик при удалении компонента")]
    public void CallHandlerWhenComponentRemoved()
    {
        // arrange
        Actor actor = fixture.Actors.CreateActor();
        fixture.Actors.AddComponent(actor.Id, new Attack { Value = 10 });

        uint? removedActorId = null;
        fixture.Actors.OnComponentRemoving<Attack>(id => removedActorId = id.Value);

        // act
        fixture.Actors.RemoveComponent<Attack>(actor.Id);

        // assert
        removedActorId.Should().Be(actor.Id.Value);
    }

    [Fact(DisplayName = "Вызывать обработчик со значением при удалении компонента")]
    public void CallHandlerWithValueWhenComponentRemoved()
    {
        // arrange
        Actor actor = fixture.Actors.CreateActor();
        fixture.Actors.AddComponent(actor.Id, new Attack { Value = 20 });

        uint? removedActorId = null;
        Attack? removedComponent = null;

        fixture.Actors.OnComponentRemoving((ActorId id, ref Attack component) =>
        {
            removedActorId = id.Value;
            removedComponent = component;
        });

        // act
        fixture.Actors.RemoveComponent<Attack>(actor.Id);

        // assert
        removedActorId.Should().Be(actor.Id.Value);
        removedComponent.Should().NotBeNull();
        removedComponent?.Value.Should().Be(20);
    }

    [Fact(DisplayName = "Вызывать обработчик при обновлении компонента")]
    public void CallHandlerWhenComponentUpdated()
    {
        // arrange
        Actor actor = fixture.Actors.CreateActor();
        fixture.Actors.AddComponent(actor.Id, new Attack { Value = 10 });

        uint? updatedActorId = null;
        Attack? oldComponent = null;
        Attack? newComponent = null;

        fixture.Actors.OnComponentUpdating((ActorId id, ref Attack oldValue, in Attack newValue) =>
        {
            updatedActorId = id.Value;
            oldComponent = oldValue;
            newComponent = newValue;
        });

        // act
        var updatedAttack = new Attack { Value = 30 };
        fixture.Actors.UpdateComponent(actor.Id, updatedAttack);

        // assert
        updatedActorId.Should().Be(actor.Id.Value);
        oldComponent.Should().NotBeNull();
        oldComponent?.Value.Should().Be(10);
        newComponent.Should().NotBeNull();
        newComponent?.Value.Should().Be(30);
    }

    [Fact(DisplayName = "Удалять компонент у актёра")]
    public void RemoveComponentFromActor()
    {
        // arrange
        Actor actor = fixture.Actors.CreateActor();
        fixture.Actors.AddComponent(actor.Id, new Attack { Value = 10 });

        // act
        bool result = fixture.Actors.RemoveComponent<Attack>(actor.Id);

        // assert
        result.Should().BeTrue();
        fixture.Actors.HasComponent<Attack>(actor.Id).Should().BeFalse();
    }

    [Fact(DisplayName = "Возвращать значение удалённого компонента")]
    public void ReturnValueOfRemovedComponent()
    {
        // arrange
        Actor actor = fixture.Actors.CreateActor();
        fixture.Actors.AddComponent(actor.Id, new Attack { Value = 25 });

        // act
        bool result = fixture.Actors.RemoveComponent(actor.Id, out Attack removedComponent);

        // assert
        result.Should().BeTrue();
        removedComponent.Value.Should().Be(25);
        fixture.Actors.HasComponent<Attack>(actor.Id).Should().BeFalse();
    }

    [Fact(DisplayName = "Пытаться добавить компонент к актёру")]
    public void TryAddComponentToActor()
    {
        // arrange
        Actor actor = fixture.Actors.CreateActor();
        var attack = new Attack { Value = 35 };

        // act
        bool result = fixture.Actors.TryAdd(actor.Id, attack);

        // assert
        result.Should().BeTrue();
        fixture.Actors.HasComponent<Attack>(actor.Id).Should().BeTrue();
        fixture.Actors.GetComponent<Attack>(actor.Id).Value.Should().Be(35);
    }

    [Fact(DisplayName = "Получать ссылку на компонент, если он существует")]
    public void GetComponentRefIfExists()
    {
        // arrange
        Actor actor = fixture.Actors.CreateActor();
        fixture.Actors.AddComponent(actor.Id, new Attack { Value = 40 });

        // act
        ref Attack componentRef = ref fixture.Actors.TryGetComponentRef<Attack>(actor.Id);
        bool isNull = Unsafe.IsNullRef(ref componentRef);

        // assert
        isNull.Should().BeFalse();
        componentRef.Value.Should().Be(40);
    }

    [Fact(DisplayName = "Возвращать null-ссылку, если компонент не существует")]
    public void ReturnNullRefIfComponentDoesNotExist()
    {
        // arrange
        Actor actor = fixture.Actors.CreateActor();

        // act
        ref Attack componentRef = ref fixture.Actors.TryGetComponentRef<Attack>(actor.Id);
        bool isNull = Unsafe.IsNullRef(ref componentRef);

        // assert
        isNull.Should().BeTrue();
    }

    [Theory(DisplayName = "Обновлять существующий компонент")]
    [AutoData]
    public void UpdateExistingComponent(int initialValue, int updatedValue)
    {
        // arrange
        Actor actor = fixture.Actors.CreateActor();
        fixture.Actors.AddComponent(actor.Id, new Attack { Value = initialValue });

        // act
        bool result = fixture.Actors.UpdateComponent(actor.Id, new Attack { Value = updatedValue });

        // assert
        result.Should().BeTrue();
        fixture.Actors.HasComponent<Attack>(actor.Id).Should().BeTrue();
        fixture.Actors.GetComponent<Attack>(actor.Id).Value.Should().Be(updatedValue);
    }

    [Theory(DisplayName = "Создавать компонент при обновлении, если он не существует")]
    [AutoData]
    public void CreateComponentWhenUpdatingIfNotExists(int value)
    {
        // arrange
        Actor actor = fixture.Actors.CreateActor();

        // act
        bool result = fixture.Actors.UpdateComponent(actor.Id, new Attack { Value = value });

        // assert
        result.Should().BeTrue();
        fixture.Actors.HasComponent<Attack>(actor.Id).Should().BeTrue();
        fixture.Actors.GetComponent<Attack>(actor.Id).Value.Should().Be(value);
    }

    [Fact(DisplayName = "Пытаться получить ассет привязанный к актёру")]
    public void TryGetBoundAsset()
    {
        // arrange
        AssetRef<UnitAsset> asset = fixture.Assets.GetAssetRef<UnitAsset>();
        Actor actor = fixture.Actors.CreateActor();
        fixture.Actors.SetBoundAsset(actor.Id, asset);

        // act
        bool success = fixture.Actors.TryGetBoundAsset(actor.Id, out Asset boundAsset);

        // assert
        success.Should().BeTrue();
        boundAsset.Id.Should().Be(asset.Id);
    }

    [Fact(DisplayName = "Возвращать false при попытке получить несуществующий привязанный ассет")]
    public void ReturnFalseWhenTryingToGetNonExistentBoundAsset()
    {
        // arrange
        Actor actor = fixture.Actors.CreateActor();

        // act
        bool success = fixture.Actors.TryGetBoundAsset(actor.Id, out Asset boundAsset);

        // assert
        success.Should().BeFalse();
        boundAsset.Should().Be(Asset.Empty);
    }

    [Fact(DisplayName = "Получать строку с ID актёра, когда актёр существует")]
    public void GetActorToStringWithId()
    {
        // arrange
        const uint expectedId = 100_500;
        Actor actor = fixture.Actors.CreateActor(expectedId);

        // act
        var actorString = actor.ToString();

        // assert
        actorString.Should().Contain(expectedId.ToString());
    }

    [Fact(DisplayName = "Актёр должен быть доступен через интерфейс IEnumerable")]
    public void ActorsShouldBeEnumerableViaContext()
    {
        // arrange
        fixture.Actors.Clear();
        Actor actor1 = fixture.Actors.CreateActor();
        Actor actor2 = fixture.Actors.CreateActor();
        Actor actor3 = fixture.Actors.CreateActor();
        ActorId[] expectedIds = [actor1.Id, actor2.Id, actor3.Id];

        // act
        ActorId[] actualIds = [.. fixture.Actors.Select(a => a.Id)];

        // assert
        actualIds.Should().BeEquivalentTo(expectedIds);
    }

    [Fact(DisplayName = "Получать экземпляр актёра по интерфейсу ActorRef")]
    public void GetActorFromActorRef()
    {
        // arrange
        Actor actor = fixture.Actors.CreateActor();
        actor.Add(new Attack { Value = 10 });

        // act
        ActorRef<Attack> actorRef = fixture.Actors.GetActorRef<Attack>(actor.Id);
        Actor retrievedActor = actorRef;

        // assert
        retrievedActor.Id.Should().Be(actor.Id);
        retrievedActor.Get<Attack>().Value.Should().Be(10);
    }

    [Fact(DisplayName = "Получать актёра с заданным предикатом")]
    public void GetActorWithPredicate()
    {
        // arrange
        fixture.Actors.Clear();
        Actor actor1 = fixture.Actors.CreateActor();
        actor1.Add(new Attack { Value = 5 });

        Actor actor2 = fixture.Actors.CreateActor();
        actor2.Add(new Attack { Value = 10 });

        Actor actor3 = fixture.Actors.CreateActor();
        actor3.Add(new Attack { Value = 15 });

        // act
        ActorRef<Attack> actorWithAttack10 =
            fixture.Actors.GetActorRef((in ActorRef<Attack> actor) => actor.Get<Attack>().Value == 10);

        // assert
        actorWithAttack10.Id
            .Should()
            .Be(actor2.Id);

        actorWithAttack10
            .Get<Attack>().Value
            .Should()
            .Be(10);
    }

    [Fact(DisplayName = "Получать единственного актёра с компонентом")]
    public void GetSingleActorWithComponent()
    {
        // arrange
        fixture.Actors.Clear();
        Actor actor = fixture.Actors.CreateActor();
        actor.Add(new NonExistentComponent());

        // act
        ActorRef<NonExistentComponent> singleActor = fixture.Actors.SingleRef<NonExistentComponent>();

        // assert
        singleActor
            .Id
            .Should()
            .Be(actor.Id);

        singleActor
            .Has<NonExistentComponent>()
            .Should()
            .BeTrue();
    }

    [Fact(DisplayName = "Генерировать исключение при получении Single, когда нет актёров с компонентом")]
    public void ThrowWhenGettingSingleWithNoMatchingActors()
    {
        // arrange
        fixture.Actors.Clear();

        // act
        Action action = () => fixture.Actors.SingleRef<NonExistentComponent1>();

        // assert
        action.Should().Throw<Exception>();
    }

    [Fact(DisplayName = "Генерировать исключение при получении Single, когда есть несколько актёров с компонентом")]
    public void ThrowWhenGettingSingleWithMultipleMatchingActors()
    {
        // arrange
        fixture.Actors.Clear();
        Actor actor1 = fixture.Actors.CreateActor();
        actor1.Add(new Attack { Value = 10 });

        Actor actor2 = fixture.Actors.CreateActor();
        actor2.Add(new Attack { Value = 20 });

        // act
        Action action = () => fixture.Actors.SingleRef<Attack>();

        // assert
        action.Should().Throw<Exception>();
    }

    [Fact(DisplayName = "Удалять привязанный ассет при установке пустого ассета")]
    public void RemoveBoundAssetWhenSettingEmptyAsset()
    {
        // arrange
        AssetRef<UnitAsset> asset = fixture.Assets.GetAssetRef<UnitAsset>();
        Actor actor = fixture.Actors.CreateActor();
        fixture.Actors.SetBoundAsset(actor.Id, asset);

        // act
        fixture.Actors.SetBoundAsset(actor.Id, Asset.Empty);
        bool hasAsset = fixture.Actors.TryGetBoundAsset(actor.Id, out _);

        // assert
        hasAsset.Should().BeFalse();
    }

    [Fact(DisplayName = "Строить актёра с указанным компонентом")]
    public void BuildActorWithSpecificComponent()
    {
        // arrange
        const int attackValue = 42;

        // act
        Actor actor = fixture.Actors.CreateActor();
        actor.Add(new Attack { Value = attackValue });

        // assert
        actor.Has<Attack>().Should().BeTrue();
        actor.Get<Attack>().Value.Should().Be(attackValue);
    }

    [Fact(DisplayName = "Генерировать исключение при попытке получить несуществующий компонент")]
    public void ThrowWhenGettingNonExistentComponent()
    {
        // arrange
        Actor actor = fixture.Actors.CreateActor();

        // act
        Action action = () => fixture.Actors.GetComponent<NonExistentComponent>(actor.Id);

        // assert
        action.Should().Throw<Exception>();
    }

    [Fact(DisplayName = "Генерировать исключение при попытке получить привязанный ассет для актёра без ассета")]
    public void ThrowWhenGettingBoundAssetForActorWithoutAsset()
    {
        // arrange
        Actor actor = fixture.Actors.CreateActor();

        // act
        Action action = () => fixture.Actors.GetBoundAsset(actor.Id);

        // assert
        action.Should().Throw<Exception>();
    }
}