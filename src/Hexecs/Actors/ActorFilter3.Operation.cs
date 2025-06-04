namespace Hexecs.Actors;

public sealed partial class ActorFilter<T1, T2, T3>
{
    private readonly struct Operation
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Operation Add(uint id, int index1, int index2, int index3)
        {
            return new Operation(true, id, index1, index2, index3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Operation Remove(uint id) => new(false, id, 0, 0, 0);

        public readonly uint Id;
        public readonly int Index1;
        public readonly int Index2;
        public readonly int Index3;
        public readonly bool IsAdd;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Operation(bool isAdd, uint id, int index1, int index2, int index3)
        {
            IsAdd = isAdd;
            Id = id;
            Index1 = index1;
            Index2 = index2;
            Index3 = index3;
        }
    }
}