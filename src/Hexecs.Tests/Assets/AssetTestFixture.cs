using Hexecs.Assets;
using Hexecs.Assets.Sources;
using Hexecs.Tests.Mocks.Assets;
using Hexecs.Worlds;

namespace Hexecs.Tests.Assets;

public sealed class AssetTestFixture : BaseFixture, IDisposable
{
    public AssetContext Assets => _assets ?? throw new Exception("Assets isn't configured");

    public World World
    {
        get => _world ?? throw new Exception("World isn't configured");
        set
        {
            if (_world != null)
            {
                _assets = null;
                _world.Dispose();
            }

            _world = value;
        }
    }

    private AssetContext? _assets;
    private World? _world;

    public Asset<T> CreateAsset<T>() where T : struct, IAssetComponent
    {
        var assetId = Asset.EmptyId;
        _world = new WorldBuilder()
            .CreateAssetData(CreateAssets)
            .CreateAssetData(loader => { assetId = loader.CreateAsset(CreateComponent<T>()).Id; })
            .Build();

        _assets = _world.Assets;
        return Assets.GetAsset<T>(assetId);
    }

    public Asset<T1> CreateAsset<T1, T2>()
        where T1 : struct, IAssetComponent
        where T2 : struct, IAssetComponent
    {
        var assetId = Asset.EmptyId;
        _world = new WorldBuilder()
            .CreateAssetData(CreateAssets)
            .CreateAssetData(loader =>
            {
                var asset = loader.CreateAsset(CreateComponent<T1>());
                asset.Set(CreateComponent<T2>());
                assetId = asset.Id;
            })
            .Build();

        _assets = _world.Assets;
        return Assets.GetAsset<T1>(assetId);
    }

    public AssetContext CreateAssetContext(Action<IAssetLoader>? assets = null)
    {
        var worldBuilder = new WorldBuilder();
        worldBuilder.CreateAssetData(CreateAssets);

        if (assets != null) worldBuilder.CreateAssetData(assets);

        _world = worldBuilder.Build();
        _assets = _world.Assets;

        return _assets;
    }

    public T CreateComponent<T>() where T : struct, IAssetComponent
    {
        object? result = null;

        if (typeof(T) == typeof(CarAsset)) result = new CarAsset(RandomInt(1, 10), RandomInt(11, 20));
        if (typeof(T) == typeof(UnitAsset)) result = new UnitAsset(RandomInt(1, 10), RandomInt(11, 20));

        return result == null
            ? throw new NotSupportedException()
            : (T)result;
    }

    private void CreateAssets(IAssetLoader loader)
    {
        var unit1 = loader.CreateAsset(UnitAsset.Alias1);
        unit1.Set(new UnitAsset(RandomInt(1, 10), RandomInt(11, 20)));

        var unit2 = loader.CreateAsset(UnitAsset.Alias2);
        unit2.Set(new UnitAsset(RandomInt(1, 10), RandomInt(11, 20)));
    }

    public void Dispose()
    {
        _assets?.Dispose();
        _world?.Dispose();
    }
}