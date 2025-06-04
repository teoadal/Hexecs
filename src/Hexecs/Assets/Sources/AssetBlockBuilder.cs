namespace Hexecs.Assets.Sources;

public sealed class AssetBlockBuilder<TArray, TItem>
    where TArray : struct, IAssetComponent, IArray<TItem>
    where TItem : struct
{
    public readonly IAssetLoader Loader;

    private readonly Func<ReadOnlyMemory<TItem>, TArray> _blockBuilder;
    private TItem[] _buffer;
    private int _length;

    public AssetBlockBuilder(
        IAssetLoader loader,
        Func<ReadOnlyMemory<TItem>, TArray> blockBuilder)
    {
        Loader = loader;

        _blockBuilder = blockBuilder;
        _buffer = ArrayUtils.Create<TItem>(4);
        _length = 0;
    }

    public void Add(in TItem item)
    {
        ArrayUtils.Insert(ref _buffer, _length, item);
        _length++;
    }

    internal TArray Flush()
    {
        var result = _blockBuilder(new ReadOnlyMemory<TItem>(_buffer, 0, _length));

        ArrayUtils.Clear(_buffer);
        _length = 0;

        return result;
    }
}