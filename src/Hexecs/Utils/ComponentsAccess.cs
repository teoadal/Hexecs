namespace Hexecs.Utils;

public readonly ref struct ComponentsAccess<T>
{
    public static ComponentsAccess<T> Empty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new ComponentsAccess<T>();
    }

    public Span<T> Values
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _values;
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
    internal ComponentsAccess(Span<uint> sparse, Span<T> values)
    {
        _sparse = sparse;
        _values = values;
    }

    public ref T this[uint id]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ref uint sparseStart = ref MemoryMarshal.GetReference(_sparse);
            ref T valuesStart = ref MemoryMarshal.GetReference(_values);

            int denseIndex = (int)Unsafe.Add(ref sparseStart, (nint)id) - 1;

            return ref Unsafe.Add(ref valuesStart, (nint)denseIndex);
        }
    }
}
