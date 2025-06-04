using Hexecs.Assets.Components;

namespace Hexecs.Assets;

public readonly struct AssetComponentRef<T>
    where T : struct, IAssetComponent
{
    public static AssetComponentRef<T> Empty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new();
    }

    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _pool == null;
    }

    private readonly int _index;
    private readonly AssetComponentPool<T> _pool;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal AssetComponentRef(AssetComponentPool<T> pool, int index)
    {
        _pool = pool;
        _index = index;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Unwrap() => ref _pool.GetByIndex(_index);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator bool(in AssetComponentRef<T> componentRef) => !componentRef.IsEmpty;
}