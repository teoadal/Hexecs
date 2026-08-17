namespace Hexecs.Arch.Utils;

public ref struct Ref<T>
    where T : struct
{
    public static Ref<T> Empty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(ref Unsafe.NullRef<T>());
    }

    public ref T Value;

    public Ref(ref T value)
    {
        Value = ref value;
    }
}