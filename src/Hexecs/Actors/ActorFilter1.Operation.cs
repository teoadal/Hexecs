namespace Hexecs.Actors;

public sealed partial class ActorFilter<T1>
{
    private readonly struct Operation
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Operation Add(uint id, int index1) => new(true, id, index1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Operation Remove(uint id) => new(false, id, 0);

        public readonly uint Id;
        public readonly int Index1;
        public readonly bool IsAdd;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Operation(bool isAdd, uint id, int index1)
        {
            IsAdd = isAdd;
            Id = id;
            Index1 = index1;
        }
    }
}