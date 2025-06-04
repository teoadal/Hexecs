namespace Hexecs.Assets.Sources;

public interface IAssetSource
{
    void Load(IAssetLoader loader);
}