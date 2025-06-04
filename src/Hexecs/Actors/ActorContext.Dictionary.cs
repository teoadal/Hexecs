using Hexecs.Actors.Relations;

namespace Hexecs.Actors;

[SuppressMessage("ReSharper", "InvertIf")]
public sealed partial class ActorContext
{
    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _length - _freeCount;
    }

    private int[] _buckets;
    private Entry[] _entries;
    private int _freeCount;
    private int _freeList;
    private int _length;

    private ref Entry AddEntry(uint key)
    {
        if (_buckets.Length == 0) ResizeEntries();

        ref var bucket = ref GetBucket(key);
        var entries = _entries;
        var i = bucket - 1;

        while ((uint)i < (uint)entries.Length)
        {
            ref readonly var existsEntry = ref entries[i];

            if (existsEntry.Key == key) ActorError.AlreadyExists(key);
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
                ResizeEntries();
                bucket = ref GetBucket(key);
                entries = _entries;
            }

            _length++;
        }

        ref var entry = ref entries[index];

        entry.Key = key;
        entry.Next = bucket - 1;

        bucket = index + 1;

        Created?.Invoke(key);

        return ref entry;
    }

    private void ClearEntry(ref Entry entry)
    {
        var actorId = entry.Key;

        ref var relationsComponent = ref TryGetComponentRef<ActorRelationComponent>(actorId);
        if (!Unsafe.IsNullRef(ref relationsComponent))
        {
            foreach (var relationId in relationsComponent)
            {
                var relationPool = _relationPools[relationId];
                relationPool?.Remove(actorId);
            }
        }
        
        ref var components = ref entry.Components;
        foreach (var componentId in components)
        {
            var componentPool = _componentPools[componentId];
            componentPool?.Remove(actorId);
        }

        components.Dispose();
    }

    private void ClearEntries()
    {
        var index = 0;
        while ((uint)index < (uint)_length)
        {
            ref var entry = ref _entries[index];
            if (entry.Next >= -1)
            {
                entry.Components.Dispose();
            }

            index++;
        }

        ArrayUtils.Clear(_buckets);
        ArrayUtils.Clear(_entries, _length);

        _freeCount = 0;
        _freeList = 0;
        _length = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref int GetBucket(uint keyHash) => ref _buckets[keyHash % (uint)_buckets.Length];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref Entry GetEntry(uint key)
    {
        if (_length > 0)
        {
            var i = _buckets[key % (uint)_buckets.Length] - 1;
            var entries = _entries;
            while ((uint)i < (uint)entries.Length)
            {
                ref var entry = ref entries[i];
                if (entry.Key == key) return ref entry;
                i = entry.Next;
            }
        }

        return ref Unsafe.NullRef<Entry>();
    }

    private ref Entry GetEntryExact(uint key)
    {
        ref var entry = ref GetEntry(key);
        if (Unsafe.IsNullRef(ref entry)) ActorError.NotFound(key);
        return ref entry;
    }

    private bool RemoveEntry(uint key)
    {
        if (_length == 0) return false;

        ref var bucket = ref GetBucket(key);
        var entries = _entries;
        var i = bucket - 1;
        var last = -1;

        while (i >= 0)
        {
            ref var entry = ref entries[i];
            if (entry.Key == key)
            {
                Destroying?.Invoke(key);
                ClearEntry(ref entry);

                if (last < 0) bucket = entry.Next + 1;
                else entries[last].Next = entry.Next;

                entry.Next = CollectionUtils.StartOfFreeList - _freeList;

                _freeCount++;
                _freeList = i;
                return true;
            }

            last = i;
            i = entry.Next;
        }

        return false;
    }

    private void ResizeEntries()
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
}