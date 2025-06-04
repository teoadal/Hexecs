namespace Hexecs.Actors;

public sealed partial class ActorFilter<T1, T2>
{
    private readonly struct Operation
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Operation Add(uint id, int index1, int index2) => new(true, id, index1, index2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Operation Remove(uint id) => new(false, id, 0, 0);

        public readonly uint Id;
        public readonly int Index1;
        public readonly int Index2;
        public readonly bool IsAdd;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Operation(bool isAdd, uint id, int index1, int index2)
        {
            IsAdd = isAdd;
            Id = id;
            Index1 = index1;
            Index2 = index2;
        }
    }
}