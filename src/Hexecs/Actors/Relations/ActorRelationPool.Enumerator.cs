namespace Hexecs.Actors.Relations;

public ref struct ActorRelationEnumerator<T>
    where T : struct
{
    public static ActorRelationEnumerator<T> Empty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(null!, ReadOnlySpan<int>.Empty);
    }

    public readonly ActorRelation<T> Current
    {
        get
        {
            var index = _indices[_currentIndex];
            var key = _pool.Keys[index];
            ref var reference = ref _pool.Values[index];
            return new ActorRelation<T>(_pool.Context, key.First == _subject ? key.Second : key.First, ref reference);
        }
    }

    public readonly int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _indices.Length;
    }

    private readonly ReadOnlySpan<int> _indices;
    private readonly ActorRelationPool<T> _pool;
    private readonly uint _subject;
    private int _currentIndex;

    internal ActorRelationEnumerator(ActorRelationPool<T> pool, ReadOnlySpan<int> indices, uint subject = 0)
    {
        _pool = pool;
        _indices = indices;
        _subject = subject;
        _currentIndex = -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ActorRelationEnumerator<T> GetEnumerator() => this;

    public void Dispose()
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool MoveNext()
    {
        _currentIndex++;
        return _currentIndex < _indices.Length;
    }
}