using Hexecsm.Utils;

namespace Hexecsm.Components;

public readonly ref struct ComponentAccess<T>
    where T : struct, IComponent
{
    public Span<T> Components
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _accessor.Values;
    }

    private readonly ActorDictionary<T>.Accessor _accessor;

    [SkipLocalsInit]
    internal ComponentAccess(ActorDictionary<T>.Accessor accessor)
    {
        _accessor = accessor;
    }

    public ref T this[ActorId actorId]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _accessor[actorId];
    }
}
