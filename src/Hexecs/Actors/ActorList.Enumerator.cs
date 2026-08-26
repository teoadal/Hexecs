using Hexecs.Actors.Components;

namespace Hexecs.Actors;

public sealed partial class ActorList<T>
{
    public ref struct Enumerator
    {
        public static Enumerator Empty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new();
        }

        public readonly ActorRef<T> Current
        {
            get
            {
                var id = _ids[_index];
                return new ActorRef<T>(
                    context: _pool.Context,
                    id: id,
                    component1: ref _pool.Get(id));
            }
        }

        public readonly int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _ids.Length;
        }

        private readonly ActorComponentPool<T> _pool;
        private readonly Span<ActorId> _ids;
        private int _index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator()
        {
            _index = -1;
            _pool = null!;
            _ids = Span<ActorId>.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(ActorList<T> list)
        {
            _index = -1;
            _pool = list._pool;
            _ids = list.AsSpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() => ++_index < _ids.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Enumerator GetEnumerator() => this;

        public Actor[] ToArray()
        {
            var count = 0;
            var actors = ArrayUtils.Create<Actor>(_ids.Length);
            foreach (var actor in this)
            {
                actors[count++] = actor;
            }

            return actors;
        }
    }
}