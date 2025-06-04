namespace Hexecs.Assets;

public sealed partial class AssetContext
{
    private struct Entry
    {
        private ushort[]? _array;
        private int _length;

        public void Add(ushort componentId)
        {
            ArrayUtils.InsertOrCreate(ref _array, _length, componentId);
            _length++;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<ushort> AsReadOnlySpan() => _length == 0
            ? ReadOnlySpan<ushort>.Empty
            : new ReadOnlySpan<ushort>(_array, 0, _length);
    }
}