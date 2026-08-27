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
        if (id == AssetId.EmptyId)
        {
            AssetError.InvalidId();
        }

        ref Entry entry = ref CollectionsMarshal.GetValueRefOrAddDefault(_entries, id, out bool exists);
        if (exists)
        {
            AssetError.AlreadyExists(id);
        }

        return ref entry;
    }

    private void ClearEntries()
    {
        foreach (Entry value in _entries.Values)
        {
            value.Dispose();
        }

        _entries.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref Entry GetEntry(uint id)
    {
        return ref CollectionsMarshal.GetValueRefOrNullRef(_entries, id);
    }

    private ref Entry GetEntryExact(uint id)
    {
        ref Entry entry = ref GetEntry(id);
        if (Unsafe.IsNullRef(ref entry))
        {
            AssetError.NotFound(id);
        }

        return ref entry;
    }
}