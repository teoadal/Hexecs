namespace Hexecs.Assets.Components;

public interface IAssetComponentPool
{
    AssetContext Context { get; }

    int Length { get; }
    
    ushort Id { get; }

    IAssetComponent Get(uint assetId);

    bool Has(uint assetId);
}