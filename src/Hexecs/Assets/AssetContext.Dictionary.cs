namespace Hexecs.Assets;

public sealed partial class AssetContext
{
    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _entries.Count;
    }

    private readonly Dictionary<uint, Entry> _entries;

    private ref Entry AddEntry(uint id)
    {
        ref var entry = ref CollectionsMarshal.GetValueRefOrAddDefault(_entries, id, out var exists);
        if (exists) AssetError.AlreadyExists(id);
        return ref entry;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref Entry GetEntry(uint id) => ref CollectionsMarshal.GetValueRefOrNullRef(_entries, id);

    private ref Entry GetEntryExact(uint id)
    {
        ref var entry = ref GetEntry(id);
        if (Unsafe.IsNullRef(ref entry)) AssetError.NotFound(id);
        return ref entry;
    }
}