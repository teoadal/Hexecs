using Hexecs.Assets;
using Hexecs.Assets.Sources;
using Hexecs.Tests.Mocks;
using Hexecs.Worlds;

namespace Hexecs.Tests.Assets;

public sealed class AssetTestFixture : BaseFixture, IDisposable
{
    public ActorContext Actors => World.Actors;
    public AssetContext Assets => World.Assets;
    public readonly World World;

    public AssetTestFixture()
    {
        World = new WorldBuilder()
            .CreateAssetData(CreateAssets)
            .Build();
    }

    public (AssetContext, World) CreateAssetContext(Action<IAssetLoader> assets)
    {
        var world = new WorldBuilder()
            .CreateAssetData(CreateAssets)
            .CreateAssetData(assets)
            .Build();

        return (world.Assets, world);
    }

    public T CreateComponent<T>() where T : struct, IAssetComponent
    {
        object? result = null;

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
        World.Dispose();
    }
}