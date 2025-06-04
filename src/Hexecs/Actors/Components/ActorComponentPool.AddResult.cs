namespace Hexecs.Actors.Components;

internal sealed partial class ActorComponentPool<T>
{
    private readonly ref struct AddResult
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AddResult Failure() => new(-1, ref Unsafe.NullRef<T>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AddResult Success(int index, ref T component) => new(index, ref component);

        public readonly ref T Component;
        public readonly int Index;

        public bool IsSuccess
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Index >= 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private AddResult(int index, ref T component)
        {
            Component = ref component;
            Index = index;
        }
    }
}