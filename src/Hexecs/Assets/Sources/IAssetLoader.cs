namespace Hexecs.Assets.Sources;

public interface IAssetLoader
{
    AssetConfigurator CreateAsset();

    AssetConfigurator CreateAsset<T1>(in T1 component1)
        where T1 : struct, IAssetComponent;

    AssetConfigurator CreateAsset<T1, T2>(in T1 component1, in T2 component2)
        where T1 : struct, IAssetComponent
        where T2 : struct, IAssetComponent;

    AssetConfigurator CreateAsset<T1, T2, T3>(in T1 component1, in T2 component2, in T3 component3)
        where T1 : struct, IAssetComponent
        where T2 : struct, IAssetComponent
        where T3 : struct, IAssetComponent;

    AssetConfigurator CreateAsset(uint id);

    AssetConfigurator CreateAsset<T1>(uint id, in T1 component1)
        where T1 : struct, IAssetComponent;

    AssetConfigurator CreateAsset<T1, T2>(uint id, in T1 component1, in T2 component2)
        where T1 : struct, IAssetComponent
        where T2 : struct, IAssetComponent;

    AssetConfigurator CreateAsset<T1, T2, T3>(uint id, in T1 component1, in T2 component2, in T3 component3)
        where T1 : struct, IAssetComponent
        where T2 : struct, IAssetComponent
        where T3 : struct, IAssetComponent;

    AssetConfigurator CreateAsset(string alias);

    AssetConfigurator CreateAsset<T1>(string alias, in T1 component1)
        where T1 : struct, IAssetComponent;

    AssetConfigurator CreateAsset<T1, T2>(string alias, in T1 component1, in T2 component2)
        where T1 : struct, IAssetComponent
        where T2 : struct, IAssetComponent;

    AssetConfigurator CreateAsset<T1, T2, T3>(string alias, in T1 component1, in T2 component2, in T3 component3)
        where T1 : struct, IAssetComponent
        where T2 : struct, IAssetComponent
        where T3 : struct, IAssetComponent;

    void EnsureNotDisposed();

    Asset GetAsset(uint assetId);

    Asset GetAsset(string alias);

    Asset<T1> GetAsset<T1>(uint assetId) where T1 : struct, IAssetComponent;

    Asset<T1> GetAsset<T1>(string alias) where T1 : struct, IAssetComponent;

    string GetAlias(uint assetId);

    uint GetId(string assetAlias);

    AssetBlockBuilder<TArray, TItem> RentBlockBuilder<TArray, TItem>(
        Func<ReadOnlyMemory<TItem>, TArray> blockBuilder)
        where TArray : struct, IAssetComponent, IArray<TItem>
        where TItem : struct;

    void ReturnBlockBuilder<TArray, TItem>(AssetBlockBuilder<TArray, TItem> builder)
        where TArray : struct, IAssetComponent, IArray<TItem>
        where TItem : struct;
}