namespace Hexecs.Utils;

public readonly ref struct ComponentsAccess<T>
{
    public static ComponentsAccess<T> Empty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new();
    }
    
    private readonly ReadOnlySpan<uint> _sparse;
    private readonly Span<T> _values;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ComponentsAccess()
    {
        _sparse = ReadOnlySpan<uint>.Empty;
        _values = Span<T>.Empty;
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
        get
        {
            ref var sparseStart = ref MemoryMarshal.GetReference(_sparse);
            ref var valuesStart = ref MemoryMarshal.GetReference(_values);

            var denseIndex = (int)Unsafe.Add(ref sparseStart, (nint)id) - 1;
            return ref Unsafe.Add(ref valuesStart, (nint)denseIndex);
        }
    }
}