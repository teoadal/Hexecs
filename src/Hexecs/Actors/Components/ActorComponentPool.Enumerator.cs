namespace Hexecs.Actors.Components;

internal sealed partial class ActorComponentPool<T>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator() => new(this);

    [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
    public ref struct Enumerator
    {
        public readonly ActorRef<T> Current
        {
            get
            {
                ref var entry = ref _pool._entries[_index];
                return new ActorRef<T>(_pool.Context, entry.Key, ref entry.Value);
            }
        }

        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        public readonly int Index
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _index;
        }

        public readonly int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _pool.Length;
        }

        private int _index;
        private readonly ActorComponentPool<T> _pool;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(ActorComponentPool<T> pool)
        {
            _index = -1;
            _pool = pool;
        }

        public bool MoveNext()
        {
            var index = _index + 1;
            var length = _pool._length;
            var entries = _pool._entries;
            while ((uint)index < (uint)length)
            {
                ref readonly var entry = ref entries[index];
                if (entry.Next >= -1)
                {
                    _index = index;
                    return true;
                }

                index++;
            }

            _index = length;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Enumerator GetEnumerator() => this;
    }
}