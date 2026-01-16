using Hexecs.Collections;

namespace Hexecs.Actors.Relations;

internal sealed partial class ActorRelationPool<T> : IActorRelationPool
    where T : struct
{
    public readonly ActorContext Context;

    public uint Id
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ActorRelationType<T>.Id;
    }

    public Type Type
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => typeof(T);
    }

    private T[] _values;
    private RelationKey[] _keys;
    private int _count;
    private readonly Dictionary<RelationKey, int> _indexMap;
    
    private Bucket<int>[]?[] _sparsePages;
    private const int PageBits = 10; // 1024
    private const int PageSize = 1 << PageBits;
    private const int PageMask = PageSize - 1;

    internal ReadOnlySpan<RelationKey> Keys => _keys.AsSpan(0, _count);
    internal T[] Values => _values;

    public ActorRelationPool(ActorContext context, int capacity = 16)
    {
        Context = context;

        capacity = HashHelper.GetPrime(capacity);

        _values = ArrayUtils.Create<T>(capacity);
        _keys = ArrayUtils.Create<RelationKey>(capacity);
        _indexMap = new Dictionary<RelationKey, int>(capacity);
        _sparsePages = [];
        _count = 0;
    }

    public ref T Add(uint subject, uint relative, in T value)
    {
        var key = new RelationKey(subject, relative);
        // Если такая связь уже есть (неважно, кто субъект, а кто цель), выбрасываем ошибку
        if (_indexMap.ContainsKey(key)) ActorError.RelationAlreadyExists<T>(subject, relative);

        if (_count >= _values.Length)
        {
            var newCapacity = _values.Length * 2;
            ArrayUtils.Resize(ref _values, newCapacity);
            ArrayUtils.Resize(ref _keys, newCapacity);
        }

        var index = _count++;
        _keys[index] = key;
        _values[index] = value;
        _indexMap[key] = index;

        // ВАЖНО: Добавляем индекс в списки ОБОИХ участников
        AddToAdjacency(subject, index);
        // Если это не связь сам с собой, добавляем и второму
        if (subject != relative)
        {
            AddToAdjacency(relative, index);
        }

        return ref _values[index];
    }

    private void AddToAdjacency(uint actorId, int index)
    {
        var pageIndex = (int)(actorId >> PageBits);
        if (pageIndex >= _sparsePages.Length)
        {
            ArrayUtils.EnsureCapacity(ref _sparsePages, pageIndex + 1);
        }

        ref var page = ref _sparsePages[pageIndex];
        page ??= new Bucket<int>[PageSize];

        ref var bucket = ref page[actorId & PageMask];
        bucket.Add(index);
    }

    private void RemoveFromAdjacency(uint actorId, int index)
    {
        var pageIndex = (int)(actorId >> PageBits);
        var page = _sparsePages[pageIndex];
        ref var bucket = ref page![actorId & PageMask];
            
        var span = bucket.AsSpan();
        for (var i = 0; i < span.Length; i++)
        {
            if (span[i] == index)
            {
                bucket.RemoveAtSwapBack(i);
                return;
            }
        }
    }

    private void ReplaceInAdjacency(uint actorId, int oldIndex, int newIndex)
    {
        var pageIndex = (int)(actorId >> PageBits);
        var page = _sparsePages[pageIndex];
        ref var bucket = ref page![actorId & PageMask];
        var span = bucket.AsSpan();
        for (var i = 0; i < span.Length; i++)
        {
            if (span[i] == oldIndex)
            {
                span[i] = newIndex;
                return;
            }
        }
    }

    public void Clear()
    {
        _indexMap.Clear();
        ArrayUtils.Clear(_values, _count);
        ArrayUtils.Clear(_keys, _count);

        foreach (var page in _sparsePages)
        {
            if (page == null) continue;
            for (var i = 0; i < page.Length; i++)
            {
                page[i].Dispose();
            }
        }

        _count = 0;
    }

    public int Count(uint subject)
    {
        var pageIndex = (int)(subject >> PageBits);
        if (pageIndex >= _sparsePages.Length) return 0;
        var page = _sparsePages[pageIndex];
        if (page == null) return 0;
        return page[subject & PageMask].Length;
    }

    public ref T Get(uint subject, uint relative)
    {
        var key = new RelationKey(subject, relative);
        if (_indexMap.TryGetValue(key, out var index))
        {
            return ref _values[index];
        }

        ActorError.RelationNotFound<T>(subject, relative);
        return ref Unsafe.NullRef<T>();
    }

    public ActorRelationEnumerator<T> GetRelations(uint subject)
    {
        var pageIndex = (int)(subject >> PageBits);
        if (pageIndex >= _sparsePages.Length) return ActorRelationEnumerator<T>.Empty;
        var page = _sparsePages[pageIndex];
        if (page == null) return ActorRelationEnumerator<T>.Empty;

        return new ActorRelationEnumerator<T>(this, page[subject & PageMask].AsReadOnlySpan(), subject);
    }

    public bool Has(uint subject, uint relative)
    {
        var key = new RelationKey(subject, relative);
        return _indexMap.ContainsKey(key);
    }

    public bool Remove(uint subject)
    {
        var pageIndex = (int)(subject >> PageBits);
        if (pageIndex >= _sparsePages.Length) return false;
        var page = _sparsePages[pageIndex];
        if (page == null) return false;

        ref var bucket = ref page[subject & PageMask];
        if (bucket.Length == 0) return false;

        var removedAny = false;
        // Итерируемся с конца бакета, так как при Remove связь удаляется из бакета 
        // (но Swap-Back в основном пуле все равно может перемешать индексы других связей в этом же бакете)
        while (bucket.Length > 0)
        {
            // Берем всегда первый индекс из бакета
            var index = bucket.AsReadOnlySpan()[0];
            var key = _keys[index];
            if (Remove(key.First, key.Second, out _))
            {
                removedAny = true;
            }
            else
            {
                // Защита от бесконечного цикла, если что-то пошло не так
                break;
            }
        }

        return removedAny;
    }

    public bool Remove(uint subject, uint relative, out T removed)
    {
        var key = new RelationKey(subject, relative);
        if (!_indexMap.TryGetValue(key, out var index))
        {
            removed = default;
            return false;
        }

        removed = _values[index];
        _indexMap.Remove(key);

        // Удаляем из списков смежности ОБОИХ участников
        RemoveFromAdjacency(key.First, index);
        if (key.First != key.Second)
        {
            RemoveFromAdjacency(key.Second, index);
        }

        var lastIndex = _count - 1;
        if (index != lastIndex)
        {
            var lastKey = _keys[lastIndex];
            var lastValue = _values[lastIndex];

            _keys[index] = lastKey;
            _values[index] = lastValue;
            _indexMap[lastKey] = index;

            // Обновляем индексы в списках смежности для перемещенной связи у ОБОИХ участников
            ReplaceInAdjacency(lastKey.First, lastIndex, index);
            if (lastKey.First != lastKey.Second)
            {
                ReplaceInAdjacency(lastKey.Second, lastIndex, index);
            }
        }

        _keys[lastIndex] = default;
        _values[lastIndex] = default;
        _count--;

        return true;
    }

    [DebuggerDisplay("{First} to {Second}")]
    internal readonly struct RelationKey : IEquatable<RelationKey>
    {
        public readonly uint First;
        public readonly uint Second;

        public RelationKey(uint first, uint second)
        {
            if (first < second)
            {
                First = first;
                Second = second;
            }
            else
            {
                First = second;
                Second = first;
            }
        }

        public override int GetHashCode() => HashCode.Combine(First, Second);

        public bool Equals(RelationKey other) => First == other.First && Second == other.Second;

        public override bool Equals(object? obj) => obj is RelationKey key && Equals(key);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Is(uint value) => First == value || Second == value;
    }

    ActorContext IActorRelationPool.Context => Context;
}