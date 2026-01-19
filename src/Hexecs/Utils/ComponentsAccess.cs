namespace Hexecs.Utils;

public readonly ref struct ComponentsAccess<T>
{
    private readonly uint[] _sparse;
    private readonly T[] _values;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ComponentsAccess()
    {
        _sparse = [];
        _values = [];
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ComponentsAccess(uint[] sparse, T[] values)
    {
        _sparse = sparse;
        _values = values;
    }

    public ref T this[uint id]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Unsafe.Add(
            ref MemoryMarshal.GetArrayDataReference(_values),
            (int)Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_sparse), (int)id) - 1);
    }
}