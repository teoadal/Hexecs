namespace Hexecs.Actors;

public sealed partial class ActorFilter<T1, T2>
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private readonly struct Entry
    {
        public readonly int Index1;
        public readonly int Index2;

        [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entry(int index1, int index2)
        {
            Index1 = index1;
            Index2 = index2;
        }
    }
}