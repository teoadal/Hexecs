using Hexecs.Utils;

namespace Hexecs.Actors.Components;

internal sealed partial class ActorComponentPool<T>
{
    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _length - _freeCount;
    }

    private int[] _buckets;
    private Entry[] _entries;
    private int _length;
    private int _freeCount;
    private int _freeList;

    private AddedEntry AddEntry(uint actorId, bool throwIfExists = true)
    {
        if (_buckets.Length == 0) Resize();

        ref var bucket = ref GetBucket(actorId);
        var entries = _entries;
        var i = bucket - 1;

        while ((uint)i < (uint)entries.Length)
        {
            ref var existsEntry = ref entries[i];
            if (existsEntry.Key == actorId)
            {
                if (throwIfExists) ActorError.ComponentExists<T>(actorId);
                return new AddedEntry(ref existsEntry, i, true);
            }

            i = existsEntry.Next;
        }

        int index;
        if (_freeCount > 0)
        {
            index = _freeList;
            _freeList = CollectionUtils.StartOfFreeList - entries[index].Next;
            _freeCount--;
        }
        else
        {
            index = _length;
            if (index == entries.Length)
            {
                Resize();
                bucket = ref GetBucket(actorId);
                entries = _entries;
            }

            _length++;
        }

        ref var entry = ref entries[index];

        entry.Key = actorId;
        entry.Next = bucket - 1;

        bucket = index + 1;

        return new AddedEntry(ref entry, index, false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref int GetBucket(uint keyHash) => ref _buckets[keyHash % (uint)_buckets.Length];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref Entry GetEntry(uint key)
    {
        if (_length == 0) return ref Unsafe.NullRef<Entry>();

        var i = _buckets[key % (uint)_buckets.Length] - 1;
        var entries = _entries;
        while ((uint)i < (uint)entries.Length)
        {
            ref var entry = ref entries[i];
            if (entry.Key == key) return ref entry;
            i = entry.Next;
        }

        return ref Unsafe.NullRef<Entry>();
    }

    private void Resize()
    {
        var length = _length;
        var newSize = HashHelper.GetPrime(length == 0 ? 4 : length << 1);

        var buckets = new int[newSize];
        var entries = new Entry[newSize];

        Array.Copy(_entries, entries, length);

        for (var i = 0; i < length; i++)
        {
            ref var entry = ref entries[i];

            if (entry.Next < -1) continue;

            ref var bucket = ref buckets[entry.Key % (uint)buckets.Length];
            entry.Next = bucket - 1;
            bucket = i + 1;
        }

        _buckets = buckets;
        _entries = entries;
    }

    private readonly ref struct AddedEntry
    {
        public readonly ref Entry Entry;
        public readonly int Index;
        public readonly bool Exists;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AddedEntry(ref Entry entry, int index, bool exists)
        {
            Index = index;
            Entry = ref entry;
            Exists = exists;
        }
    }
}