namespace Hexecs.Tests.Assets;

public sealed class AssetContextShould(AssetTestFixture fixture) : IClassFixture<AssetTestFixture>
{
    [Fact]
    public void GetAssetByAlias()
    {
        // arrange

        var alias = fixture.RandomString();
        uint? assetId = null;
        fixture.CreateAssetContext(loader =>
        {
            var asset = loader.CreateAsset(alias);
            assetId = asset.Id;
        });

        // act

        var actual = fixture.Assets.Invoking(ctx => ctx.GetAsset(alias))
            .Should()
            .NotThrow()
            .Which;

        // assert

        actual.Id
            .Should()
            .Be(assetId);
    }

    [Fact]
    public void Throw_IfAssetByAlias_NotFound()
    {
        // act && assert

        var context = fixture.CreateAssetContext();
        context.Invoking(ctx => ctx.GetAsset(fixture.RandomString()))
            .Should()
            .Throw<Exception>();
    }
}