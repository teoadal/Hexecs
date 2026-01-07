using Hexecs.Actors.Components;
using Hexecs.Collections;

namespace Hexecs.Actors.Relations;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    public readonly ReadOnlySpan<uint> AsReadOnlySpan() => _array == null
        ? ReadOnlySpan<uint>.Empty
        : _array.AsSpan(0, _length);

    public void Dispose()
    {
        var arr = _array;
        if (arr != null)
        {
            _array = null; // Защита от двойного Dispose
            ArrayPool<uint>.Shared.Return(arr);
        }

        _length = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ArrayEnumerator<uint> GetEnumerator() => _array == null
        ? ArrayEnumerator<uint>.Empty
        : new ArrayEnumerator<uint>(_array, _length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Has(uint relationId) => _length != 0 && AsReadOnlySpan().Contains(relationId);

    public bool Remove(uint relationId)
    {
        var span = AsReadOnlySpan();
        for (var i = 0; i < span.Length; i++)
        {
            if (span[i] != relationId) continue;

            // Swap-Back: $O(1)$
            var lastIndex = --_length;
            if (i < lastIndex)
            {
                _array![i] = _array[lastIndex];
            }

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