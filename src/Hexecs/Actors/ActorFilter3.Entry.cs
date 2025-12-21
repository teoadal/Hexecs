namespace Hexecs.Actors;

public sealed partial class ActorFilter<T1, T2, T3>
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private readonly struct Entry
    {
        public readonly int Index1;
        public readonly int Index2;
        public readonly int Index3;

        [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entry(int index1, int index2, int index3)
        {
            Index1 = index1;
            Index2 = index2;
            Index3 = index3;
        }
    }
}