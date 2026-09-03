using Hexecsm.Accessors;

namespace Hexecsm.Components;

public readonly struct Components<T>
    where T : struct, IComponent
{
    private readonly ComponentPool<T> _pool;

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Components(ComponentPool<T> pool)
    {
        _pool = pool;
    }

    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _pool.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public KeyValueAccessor<T> GetKeyValues()
    {
        return _pool.GetKeyValues();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public KeyAccessor GetKeys()
    {
        return _pool.GetKeys();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueAccessor<T> GetValues()
    {
        return _pool.GetValues();
    }
}
