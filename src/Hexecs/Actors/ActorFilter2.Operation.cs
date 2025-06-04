namespace Hexecs.Actors;

public sealed partial class ActorFilter<T1, T2>
{
    private readonly struct Operation
    {
        private const int ClearFlag = -1;
        private const int RemoveFlag = -2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Operation Add(uint id, int index1, int index2)
        {
            return new Operation(id, index1, index2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Operation Clear() => new(0, ClearFlag, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Operation Remove(uint id) => new(id, RemoveFlag, 0);

        public readonly uint Id;
        public readonly int Index1;
        public readonly int Index2;

        public bool IsAdd
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Index1 >= 0;
        }

        public bool IsClear
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Index1 == ClearFlag;
        }

        public bool IsRemove
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Index1 == RemoveFlag;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Operation(uint id, int index1, int index2)
        {
            Id = id;
            Index1 = index1;
            Index2 = index2;
        }
    }
}