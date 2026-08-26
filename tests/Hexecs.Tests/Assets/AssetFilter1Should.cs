using Hexecs.Assets;
using Hexecs.Assets.Sources;
using Hexecs.Tests.Mocks.Assets;

namespace Hexecs.Tests.Assets;

public sealed class AssetFilter1Should(AssetTestFixture fixture) : IClassFixture<AssetTestFixture>
{
    [Fact(DisplayName = "Фильтр ассетов должен содержать все созданные ассеты")]
    public void ContainsAllAssets()
    {
        // arrange

        var assetIds = new List<AssetId>();

        AssetContext context = fixture.CreateAssetContext(loader =>
        {
            for (var i = 1; i < 100; i++)
            {
                AssetConfigurator asset = loader.CreateAsset(new CarAsset(i, i));
                assetIds.Add(asset.Id);
            }
        });

        Asset[] expectedAssets = assetIds
            .Select(context.GetAsset)
            .ToArray();

        // act

        AssetFilter<CarAsset> filter = context.Filter<CarAsset>();
        Asset[] actualActors = filter.ToArray();

        // assert

        actualActors
            .Should()
            .Contain(expectedAssets);
    }

    [Fact(DisplayName = "Фильтр ассетов можно перебирать как AssetRef")]
    public void AssetFilterShouldEnumerable()
    {
        // arrange

        var expectedIds = new Dictionary<AssetId, CarAsset>();

        AssetContext context = fixture.CreateAssetContext(loader =>
        {
            for (var i = 0; i < 100; i++)
            {
                var component = new CarAsset(i, i);
                AssetConfigurator asset = loader.CreateAsset(component);

                expectedIds.Add(asset.Id, component);
            }
        });

        // act

        AssetFilter<CarAsset> filter = context.Filter<CarAsset>();

        // assert

        var actualIds = new List<AssetId>();

        foreach (AssetRef<CarAsset> asset in filter)
        {
            actualIds.Add(asset.Id);
            asset
                .Component1
                .Should()
                .Be(expectedIds[asset.Id]);
        }

        filter.Length
            .Should()
            .Be(expectedIds.Count);

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
        AssetContext context = fixture.CreateAssetContext();

        // act
        AssetFilter<CarAsset> filter = context.Filter<CarAsset>();

        // assert
        filter.Length
            .Should()
            .Be(0);
    }

    [Fact(DisplayName = "Фильтр должен учитывать constraint")]
    public void FilterWithConstraint()
    {
        var notExpectedIds = new List<AssetId>();
        var expectedId = AssetId.Empty;

        // arrange
        AssetContext context = fixture.CreateAssetContext(loader =>
        {
            notExpectedIds.Add(
                loader.CreateAsset(
                        new CarAsset(10, 10),
                        new UnitAsset())
                    .Id);

            notExpectedIds.Add(loader.CreateAsset(new CarAsset(30, 30)).Id);

            expectedId = loader.CreateAsset(
                    new CarAsset(20, 20),
                    new BuildingAsset())
                .Id;
        });

        // act

        AssetFilter<CarAsset> filter = context.Filter<CarAsset>(constraint => constraint
            .Exclude<UnitAsset>()
            .Include<BuildingAsset>());

        // assert

        filter.Length.Should().Be(1);

        filter
            .Contains(expectedId)
            .Should()
            .BeTrue();

        foreach (AssetId notExpectedId in notExpectedIds)
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
        AssetContext context = fixture.CreateAssetContext(loader => { loader.CreateAsset(new CarAsset(1, 1)); });

        AssetFilter<CarAsset> filter = context.Filter<CarAsset>();

        // act

        Action act = () => filter.Get(new AssetId(999)); // Несуществующий ID

        // assert
        act
            .Should()
            .Throw<Exception>();
    }

    [Fact(DisplayName = "Contains возвращает корректный статус наличия ассета")]
    public void ContainsReturnsCorrectStatus()
    {
        // arrange

        var existingId = AssetId.Empty;
        AssetContext context = fixture.CreateAssetContext(loader =>
        {
            AssetConfigurator asset = loader.CreateAsset(new CarAsset(1, 1));
            existingId = asset.Id;
        });

        AssetFilter<CarAsset> filter = context.Filter<CarAsset>();

        // act & assert
        filter
            .Contains(existingId)
            .Should()
            .BeTrue();

        filter
            .Contains(new AssetId(existingId.Value + 100))
            .Should()
            .BeFalse();
    }
}
