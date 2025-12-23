using Hexecs.Tests.Mocks;

namespace Hexecs.Tests.Assets;

public sealed class AssetFilter1Should(AssetTestFixture fixture) : IClassFixture<AssetTestFixture>
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
                var asset = loader.CreateAsset(new CarAsset(i, i));
                assetIds.Add(asset.Id);
            }
        });

        var expectedAssets = assetIds.Select(id => context.GetAsset(id)).ToArray();

        // act

        var filter = context.Filter<CarAsset>();
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
        var expectedIds = new Dictionary<uint, CarAsset>();

        var context = fixture.CreateAssetContext(loader =>
        {
            for (var i = 0; i < 100; i++)
            {
                var component = new CarAsset(i, i);
                var asset = loader.CreateAsset(component);

                expectedIds.Add(asset.Id, component);
            }
        });

        // act

        var filter = context.Filter<CarAsset>();

        // assert

        var actualIds = new List<uint>();
        foreach (var asset in filter)
        {
            actualIds.Add(asset.Id);
            asset
                .Component1
                .Should().Be(expectedIds[asset.Id]);
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
        var context = fixture.CreateAssetContext(loader => loader
            .CreateAsset(new NonExistentAsset()));

        // act
        var filter = context.Filter<CarAsset>();

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
            notExpectedIds.Add(loader.CreateAsset(
                new CarAsset(10, 10),
                new UnitAsset()).Id);

            notExpectedIds.Add(loader.CreateAsset(
                new CarAsset(30, 30)).Id);

            expectedId = loader.CreateAsset(
                new CarAsset(20, 20),
                new BuildingAsset()).Id;
        });

        // act

        var filter = context.Filter<CarAsset>(constraint => constraint
            .Exclude<UnitAsset>()
            .Include<BuildingAsset>());

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
        var context = fixture.CreateAssetContext(loader => { loader.CreateAsset(new CarAsset(1, 1)); });

        var filter = context.Filter<CarAsset>();

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
            var asset = loader.CreateAsset(new CarAsset(1, 1));
            existingId = asset.Id;
        });

        var filter = context.Filter<CarAsset>();

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