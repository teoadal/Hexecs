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
        var expectedIds = new List<uint>();

        var (context, world) = fixture.CreateAssetContext(loader =>
        {
            for (int i = 0; i < 100; i++)
            {
                var asset = loader.CreateAsset(new CarAsset(i, i));
                expectedIds.Add(asset.Id);
            }
        });

        // act

        var filter = context.Filter<CarAsset>();
        var actualIds = new List<uint>();
        foreach (var asset in filter)
        {
            actualIds.Add(asset.Id);
        }

        // assert

        filter.Length
            .Should().Be(expectedIds.Count);
        
        actualIds
            .Should()
            .HaveCount(expectedIds.Count);

        actualIds
            .Should()
            .Contain(expectedIds);

        world.Dispose();
    }
}