using Hexecs.Tests.Mocks;

namespace Hexecs.Tests.Assets;

public sealed class AssetFilter3Should(AssetTestFixture fixture) : IClassFixture<AssetTestFixture>
{
    [Fact(DisplayName = "Фильтр ассетов должен содержать все созданные ассеты")]
    public void ContainsAllAssets()
    {
        // arrange 
        var assetIds = new List<uint>();

        var context = fixture.CreateAssetContext(loader =>
        {
            for (int i = 1; i < 100; i++)
            {
                var asset = loader.CreateAsset(
                    new CarAsset(i, i),
                    new DecisionAsset(i, i),
                    new UnitAsset(i, i));
                assetIds.Add(asset.Id);
            }
        });

        var expectedAssets = assetIds.Select(id => context.GetAsset(id)).ToArray();

        // act

        var filter = context.Filter<CarAsset, UnitAsset, DecisionAsset>();
        var actualActors = filter.ToArray();

        // assert

        actualActors
            .Should()
            .Contain(expectedAssets);
    }

    [Fact(DisplayName = "Фильтр ассетов можно перебирать как AssetRef")]
    public void AssetFilterShouldEnumerable()
    {
        // arrange 
        var expectedIds = new Dictionary<uint, (CarAsset, DecisionAsset, UnitAsset)>();

        var context = fixture.CreateAssetContext(loader =>
        {
            for (var i = 1; i < 100; i++)
            {
                var component1 = new CarAsset(i, i);
                var component2 = new DecisionAsset(i, i);
                var component3 = new UnitAsset(i, i);
                var asset = loader.CreateAsset(component1, component2, component3);

                expectedIds.Add(asset.Id, (component1, component2, component3));
            }
        });

        // act

        var filter = context.Filter<CarAsset, DecisionAsset, UnitAsset>();

        // assert

        var actualIds = new List<uint>();
        foreach (var asset in filter)
        {
            actualIds.Add(asset.Id);
            asset
                .Component1
                .Should().Be(expectedIds[asset.Id].Item1);

            asset
                .Component2
                .Should().Be(expectedIds[asset.Id].Item2);

            asset
                .Component3
                .Should().Be(expectedIds[asset.Id].Item3);
        }

        filter.Length
            .Should().Be(expectedIds.Count);

        actualIds
            .Should()
            .HaveCount(expectedIds.Count);

        actualIds
            .Should()
            .Contain(expectedIds.Keys);
    }

    [Fact(DisplayName = "Фильтр должен быть пустым, если компоненты заданного типа отсутствуют")]
    public void EmptyFilterWhenNoComponentsExist()
    {
        // arrange
        var context = fixture.CreateAssetContext();

        // act
        var filter = context.Filter<CarAsset, DecisionAsset, UnitAsset>();

        // assert
        filter.Length
            .Should()
            .Be(0);
    }

    [Fact(DisplayName = "Фильтр должен учитывать constraint")]
    public void FilterWithConstraint()
    {
        var notExpectedIds = new List<uint>();
        uint expectedId = 0;

        // arrange
        var context = fixture.CreateAssetContext(loader =>
        {
            var asset = loader.CreateAsset(new CarAsset(10, 10), new DecisionAsset(), new UnitAsset());
            asset.Set(new BuildingAsset());
            notExpectedIds.Add(asset.Id);

            asset = loader.CreateAsset(new CarAsset(10, 10), new DecisionAsset(), new UnitAsset());
            asset.Set(new NonExistentAsset());
            notExpectedIds.Add(asset.Id);

            asset = loader.CreateAsset(new CarAsset(10, 10), new DecisionAsset(), new UnitAsset());
            asset.Set(new SubjectAsset());
            expectedId = asset.Id;
        });

        // act

        var filter = context.Filter<CarAsset, DecisionAsset, UnitAsset>(constraint => constraint
            .Exclude<BuildingAsset>()
            .Include<SubjectAsset>());

        // assert

        filter.Length.Should().Be(1);

        filter
            .Contains(expectedId)
            .Should()
            .BeTrue();

        foreach (var notExpectedId in notExpectedIds)
        {
            filter.Contains(notExpectedId)
                .Should()
                .BeFalse();
        }
    }

    [Fact(DisplayName = "Метод Get должен выбрасывать исключение, если ассет не найден в фильтре")]
    public void GetThrowsExceptionWhenNotFound()
    {
        // arrange
        var context = fixture.CreateAssetContext(loader => loader
            .CreateAsset(new CarAsset(1, 1), new DecisionAsset(), new UnitAsset()));

        var filter = context.Filter<CarAsset, DecisionAsset, UnitAsset>();

        // act

        Action act = () => filter.Get(999); // Несуществующий ID

        // assert
        act
            .Should()
            .Throw<Exception>();
    }

    [Fact(DisplayName = "Contains возвращает корректный статус наличия ассета")]
    public void ContainsReturnsCorrectStatus()
    {
        // arrange
        uint existingId = 0;
        var context = fixture.CreateAssetContext(loader =>
        {
            var asset = loader.CreateAsset(new CarAsset(1, 1), new DecisionAsset(), new UnitAsset());
            existingId = asset.Id;
        });

        var filter = context.Filter<CarAsset, DecisionAsset, UnitAsset>();

        // act & assert
        filter
            .Contains(existingId)
            .Should()
            .BeTrue();

        filter
            .Contains(existingId + 100)
            .Should()
            .BeFalse();
    }
}