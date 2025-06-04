using Hexecs.Actors.Components;
using Hexecs.Collections;

namespace Hexecs.Actors.Relations;

internal struct ActorRelationComponent(int capacity) : IActorComponent, IDisposable
{
    public static ActorRelationComponent Create(uint actorId) => new(4);

    public static ActorComponentConfiguration<ActorRelationComponent> CreatePoolConfiguration()
    {
        return new ActorComponentConfiguration<ActorRelationComponent>(
            null,
            null,
            DisposeHandler,
            ActorRelationComponentConverter.Instance);
    }

    public static void DisposeHandler(ref ActorRelationComponent component)
    {
        component.Dispose();
    }

    private uint[]? _array = capacity > 0 ? ArrayPool<uint>.Shared.Rent(capacity) : null;
    private int _length = 0;

    public void Add(uint relationId)
    {
        ArrayUtils.InsertOrCreate(ref _array, ArrayPool<uint>.Shared, _length, in relationId);
        _length++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ReadOnlySpan<uint> AsReadOnlySpan() => _length == 0
        ? ReadOnlySpan<uint>.Empty
        : new ReadOnlySpan<uint>(_array, 0, _length);

    public void Dispose()
    {
        if (_array is { Length: > 0 }) ArrayPool<uint>.Shared.Return(_array);

        _array = [];
        _length = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ArrayEnumerator<uint> GetEnumerator() => _array == null
        ? ArrayEnumerator<uint>.Empty
        : new ArrayEnumerator<uint>(_array, _length);

    public readonly bool Has(uint relationId)
    {
        if (_length == 0) return false;

        foreach (var exists in AsReadOnlySpan())
        {
            if (exists == relationId) return true;
        }

        return false;
    }

    public bool Remove(uint relationId)
    {
        if (_length == 0) return false;

        var span = AsReadOnlySpan();
        for (var i = 0; i < span.Length; i++)
        {
            if (span[i] != relationId) continue;

            ArrayUtils.Cut(_array!, i);
            _length--;

            return true;
        }

        return false;
    }

    public bool TryAdd(uint relationId)
    {
        var has = Has(relationId);
        if (has) return false;

        Add(relationId);
        return true;
    }
}