using Hexecs.Assets;
using Hexecs.Assets.Sources;

namespace Hexecs.Tests.Assets;

public sealed class AssetContextShould(AssetTestFixture fixture) : IClassFixture<AssetTestFixture>
{
    [Fact]
    public void GetAssetByAlias()
    {
        // arrange

        string alias = fixture.RandomString();
        var assetId = AssetId.Empty;
        fixture.CreateAssetContext(loader =>
        {
            AssetConfigurator asset = loader.CreateAsset(alias);
            assetId = asset.Id;
        });

        // act

        Asset actual = fixture.Assets.Invoking(ctx => ctx.GetAsset(alias))
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

        AssetContext context = fixture.CreateAssetContext();
        context.Invoking(ctx => ctx.GetAsset(fixture.RandomString()))
            .Should()
            .Throw<Exception>();
    }
}
