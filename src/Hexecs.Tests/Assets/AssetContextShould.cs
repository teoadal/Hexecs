namespace Hexecs.Tests.Assets;

public sealed class AssetContextShould(AssetTestFixture fixture) : IClassFixture<AssetTestFixture>
{
    [Fact]
    public void GetAssetByAlias()
    {
        // arrange

        var alias = fixture.RandomString();
        uint? assetId = null;
        var (assets, world) = fixture.CreateAssetContext(loader =>
        {
            var asset = loader.CreateAsset(alias);
            assetId = asset.Id;
        });

        // act

        var actual = assets.Invoking(ctx => ctx.GetAsset(alias))
            .Should()
            .NotThrow()
            .Which;

        // assert

        actual.Id
            .Should()
            .Be(assetId);

        world.Dispose();
    }
    
    [Fact]
    public void Throw_IfAssetByAlias_NotFound()
    {
        // act && assert

        fixture.Assets.Invoking(ctx => ctx.GetAsset(fixture.RandomString()))
            .Should()
            .Throw<Exception>();
    }
}