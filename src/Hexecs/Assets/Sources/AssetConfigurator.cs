namespace Hexecs.Assets.Sources;

public readonly struct AssetConfigurator
{
    /// <summary>
    /// Идентификатор ассета, который уже присвоен ему при создании
    /// </summary>
    public uint Id
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _id;
    }

    public IAssetLoader Loader
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _loader;
    }

    private readonly uint _id;
    private readonly AssetContext.Loader _loader;

    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal AssetConfigurator(uint id, AssetContext.Loader loader)
    {
        _id = id;
        _loader = loader;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has<T>() where T : struct, IAssetComponent => _loader.Context.HasComponent<T>(_id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref readonly T Get<T>() where T : struct, IAssetComponent => ref _loader.Context.GetComponent<T>(_id);

    public void Set<T>(in T component)
        where T : struct, IAssetComponent
    {
        _loader.SetComponent(_id, in component);
    }

    public void SetBlock<TArray, TItem>(
        Func<ReadOnlyMemory<TItem>, TArray> blockBuilder,
        Action<AssetBlockBuilder<TArray, TItem>> block)
        where TArray : struct, IAssetComponent, IArray<TItem>
        where TItem : struct
    {
        var builder = Loader.RentBlockBuilder(blockBuilder);
        block(builder);

        Set(builder.Flush());

        Loader.ReturnBlockBuilder(builder);
    }
}