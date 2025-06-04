using Hexecs.Utils;

namespace Hexecs.Actors;

public sealed partial class ActorFilter<T1>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator()
    {
        _postponedReadersCount++;
        return new Enumerator(this);
    }

    public ref struct Enumerator
    {
        public readonly ActorRef<T1> Current
        {
            get
            {
                var filter = _filter;
                var (actorId, entry) = _enumerator.Current;
                return new ActorRef<T1>(
                    filter.Context,
                    actorId,
                    ref filter._pool1.GetByIndex(entry.Index1));
            }
        }

        public readonly int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _filter.Length;
        }

        private readonly ActorFilter<T1> _filter;
        private Dictionary<uint, Entry>.Enumerator _enumerator;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(ActorFilter<T1> filter)
        {
            _filter = filter;
            _enumerator = filter._dictionary.GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() => _enumerator.MoveNext();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _filter.ProcessPostponedUpdates();
            _enumerator.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Enumerator GetEnumerator() => this;

        public Actor[] ToArray()
        {
            var count = 0;
            var actors = ArrayUtils.Create<Actor>(_filter.Length);
            foreach (var actor in this)
            {
                actors[count++] = actor;
            }

            Dispose();
            return actors;
        }
    }
}