namespace Hexecs.Assets;

public sealed partial class AssetFilter<T1, T2>
{
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly struct Entry(int index1, int index2)
    {
        public readonly int Index1 = index1;
        public readonly int Index2 = index2;
    }
}