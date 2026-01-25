namespace Hexecs.Actors;

public sealed partial class ActorFilter<T1, T2>
{
    [StructLayout(LayoutKind.Sequential)]
    private readonly struct Operation
    {
        private const byte TypeAdd = 1;
        private const byte TypeRemove = 2;
        private const byte TypeClear = 3;

        public readonly uint Id;
        public readonly byte Type;

        public bool IsAdd => Type == TypeAdd;
        public bool IsClear => Type == TypeClear;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Operation Add(uint id) => new(id, TypeAdd);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Operation Remove(uint id) => new(id, TypeRemove);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Operation Clear() => new(0, TypeClear);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Operation(uint id, byte type)
        {
            Id = id;
            Type = type;
        }
    }
}