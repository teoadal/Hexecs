using Hexecs.Actors.Components;

namespace Hexecs.Actors;

public sealed partial class ActorContext
{
    public ref struct ComponentEnumerator
    {
        public static ComponentEnumerator Empty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new();
        }

        public readonly IActorComponent Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _pools[_componentIds[_index]]!.Get(_actorId);
        }

        public readonly int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _componentIds.Length;
        }
        
        private int _index;
        private readonly uint _actorId;
        private readonly ReadOnlySpan<ushort> _componentIds;
        private readonly IActorComponentPool?[] _pools;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ComponentEnumerator()
        {
            _index = -1;
            _actorId = 0;
            _componentIds = ReadOnlySpan<ushort>.Empty;
            _pools = [];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ComponentEnumerator(uint actorId, IActorComponentPool?[] pools, ReadOnlySpan<ushort> componentIds)
        {
            _index = -1;
            _actorId = actorId;
            _componentIds = componentIds;
            _pools = pools;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() => ++_index < _componentIds.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ComponentEnumerator GetEnumerator() => this;

        public readonly IActorComponent[] ToArray()
        {
            var ids = _componentIds;

            if (ids.Length == 0) return [];

            var array = ArrayUtils.Create<IActorComponent>(ids.Length);
            for (var i = 0; i < ids.Length; i++)
            {
                array[i] = _pools[ids[i]]!.Get(_actorId);
            }

            return array;
        }
    }
}