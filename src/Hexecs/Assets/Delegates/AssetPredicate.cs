namespace Hexecs.Assets.Delegates;

public delegate bool AssetPredicate<T1>(in AssetRef<T1> component)
    where T1 : struct, IAssetComponent;

public delegate bool AssetPredicate<T1, T2>(in AssetRef<T1, T2> component)
    where T1 : struct, IAssetComponent
    where T2 : struct, IAssetComponent;
    
public delegate bool AssetPredicate<T1, T2, T3>(in AssetRef<T1, T2, T3> component)
    where T1 : struct, IAssetComponent
    where T2 : struct, IAssetComponent
    where T3 : struct, IAssetComponent;