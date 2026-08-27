namespace Hexecs.Actors;

public sealed partial class ActorFilter<T1, T2, T3>
{
    [StructLayout(LayoutKind.Sequential)]
    private readonly struct Operation
    {
        private const byte TypeAdd = 1;
        private const byte TypeRemove = 2;
        private const byte TypeClear = 3;

        public readonly ActorId Id;
        public readonly byte Type;

        public bool IsAdd => Type == TypeAdd;
        public bool IsClear => Type == TypeClear;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Operation Add(ActorId id)
        {
            return new Operation(id, TypeAdd);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Operation Remove(ActorId id)
        {
            return new Operation(id, TypeRemove);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Operation Clear()
        {
            return new Operation(ActorId.Empty, TypeClear);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Operation(ActorId id, byte type)
        {
            Id = id;
            Type = type;
        }
    }
}
