namespace Hexecs.Assets;

public sealed partial class AssetFilter<T1>
{
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly struct Entry(int index1)
    {
        public readonly int Index1 = index1;
    }
}