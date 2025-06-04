namespace Hexecs.Assets.Sources;

internal sealed class ActionAssetLoader(int order, Action<IAssetLoader> source) : IAssetSource, IHaveOrder
{
    public int Order
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => order;
    }

    public void Load(IAssetLoader loader) => source(loader);
}