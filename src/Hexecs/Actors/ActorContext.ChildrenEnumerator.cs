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
            get => new(_context, _children[_index].Id);
        }

        public readonly int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _children.Length;
        }

        private int _index;
        private readonly Span<ActorNode> _children;
        private readonly ActorContext _context;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ChildrenEnumerator()
        {
            _index = -1;
            _children = Span<ActorNode>.Empty;
            _context = null!;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ChildrenEnumerator(ActorContext context, Span<ActorNode> children)
        {
            _index = -1;
            _children = children;
            _context = context;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int Count() => _children.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() => ++_index < _children.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ChildrenEnumerator GetEnumerator() => this;

        public Actor[] ToArray()
        {
            var count = 0;
            var children = ArrayUtils.Create<Actor>(_children.Length);
            foreach (var actor in this)
            {
                children[count++] = actor;
            }

            _index = 0; // reset enumerator
            return children;
        }
    }
}