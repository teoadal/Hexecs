namespace Hexecs.Actors.Relations;

public struct ActorRelationEnumerator<T>
    where T : struct
{
    public static ActorRelationEnumerator<T> Empty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(null!, 0);
    }

    public readonly ActorRelation<T> Current
    {
        get
        {
            var current = _enumerator.Current;
            ref var reference = ref CollectionsMarshal.GetValueRefOrNullRef(_pool.Relations, current);
            return new ActorRelation<T>(_pool.Context, current.Second, ref reference);
        }
    }

    public readonly int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _pool.Count(_subject);
    }

    private Dictionary<ActorRelationPool<T>.RelationKey, T>.KeyCollection.Enumerator _enumerator;
    private readonly ActorRelationPool<T> _pool;
    private readonly uint _subject;

    internal ActorRelationEnumerator(ActorRelationPool<T>? pool, uint subject)
    {
        _pool = pool!;
        _enumerator = pool == null
            ? new Dictionary<ActorRelationPool<T>.RelationKey, T>.KeyCollection.Enumerator()
            : pool.Relations.Keys.GetEnumerator();
        _subject = subject;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ActorRelationEnumerator<T> GetEnumerator() => this;

    public void Dispose()
    {
        _enumerator.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool MoveNext()
    {
        while (_enumerator.MoveNext())
        {
            if (_enumerator.Current.Is(_subject)) return true;
        }

        return false;
    }
}