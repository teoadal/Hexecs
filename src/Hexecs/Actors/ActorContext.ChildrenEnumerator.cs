using Hexecs.Actors.Nodes;

namespace Hexecs.Actors;

public sealed partial class ActorContext
{
    public ref struct ChildrenEnumerator
    {
        public static ChildrenEnumerator Empty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new();
        }

        public readonly Actor Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(_context, _currentId);
        }

        public readonly int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (int)_count;
        }

        private readonly ActorContext _context;
        private uint _currentId;
        private uint _nextId;
        private readonly uint _count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ChildrenEnumerator(ActorContext context, uint firstChildId, uint count)
        {
            _context = context;
            _currentId = 0;
            _nextId = firstChildId;
            _count = count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            if (_nextId == 0) return false;

            _currentId = _nextId;
            ref var node = ref _context.GetComponent<ActorNodeComponent>(_currentId);
            _nextId = node.NextSiblingId;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ChildrenEnumerator GetEnumerator() => this;

        public Actor[] ToArray()
        {
            if (_count == 0) return [];

            var result = new Actor[_count];
            var index = 0;

            foreach (var actor in this)
            {
                result[index++] = actor;
            }

            return result;
        }
    }
}