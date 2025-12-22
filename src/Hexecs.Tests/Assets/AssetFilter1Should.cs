using Hexecs.Tests.Mocks;

namespace Hexecs.Tests.Assets;

public sealed class AssetFilter1Should(AssetTestFixture fixture) : IClassFixture<AssetTestFixture>
{
    [Fact(DisplayName = "Фильтр ассетов должен содержать все созданные ассеты")]
    public void ContainsAllAssets()
    {
        // arrange 
        var assetIds = new List<uint>();

        var (context, world) = fixture.CreateAssetContext(loader =>
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

        world.Dispose();
    }

    [Fact(DisplayName = "Фильтр ассетов можно перебирать как AssetRef")]
    public void AssetFilterShouldEnumerable()
    {
        // arrange 
        var expectedIds = new Dictionary<uint, CarAsset>();

        var (context, world) = fixture.CreateAssetContext(loader =>
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

        world.Dispose();
    }
}