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

    internal readonly Dictionary<RelationKey, T> Relations;

    public ActorRelationPool(ActorContext context, int capacity = 16)
    {
        Context = context;

        capacity = HashHelper.GetPrime(capacity);

        Relations = new Dictionary<RelationKey, T>(capacity);
    }

    public ref T Add(uint subject, uint relative, in T value)
    {
        var key = new RelationKey(subject, relative);
        ref var relation = ref CollectionsMarshal.GetValueRefOrAddDefault(Relations, key, out var exists);
        if (exists) ActorError.RelationAlreadyExists<T>(subject, relative);

        relation = value;
        return ref relation;
    }

    public void Clear() 
    {
        Relations.Clear();
    }

    public int Count(uint subject)
    {
        var result = 0;

        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var key in Relations.Keys)
        {
            if (key.First == subject) result++;
        }

        return result;
    }

    public ref T Get(uint subject, uint relative)
    {
        var key = new RelationKey(subject, relative);
        ref var relation = ref CollectionsMarshal.GetValueRefOrNullRef(Relations, key);
        if (Unsafe.IsNullRef(ref relation)) ActorError.RelationNotFound<T>(subject, relative);

        return ref relation;
    }

    public ActorRelationEnumerator<T> GetRelations(uint subject) => new(this, subject);

    public bool Has(uint subject, uint relative)
    {
        var key = new RelationKey(subject, relative);
        return Relations.ContainsKey(key);
    }

    public bool Remove(uint subject)
    {
        var arrayPool = ArrayPool<RelationKey>.Shared;
        var buffer = arrayPool.Rent(16);
        var length = 0;

        foreach (var key in Relations.Keys)
        {
            if (!key.Is(subject)) continue;

            ArrayUtils.Insert(ref buffer, arrayPool, length, key);
            length++;
        }

        foreach (var key in buffer.AsSpan(0, length))
        {
            Relations.Remove(key);
        }

        arrayPool.Return(buffer);

        return length > 0;
    }

    public bool Remove(uint subject, uint relative, out T removed)
    {
        var key = new RelationKey(subject, relative);
        return Relations.Remove(key, out removed);
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