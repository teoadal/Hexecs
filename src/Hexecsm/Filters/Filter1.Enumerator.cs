using Hexecsm.Accessors;

namespace Hexecsm.Filters;

public sealed partial class Filter<T1>
{
    public ref struct Enumerator
    {
        public ActorRef<T1> Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ref readonly ActorId actorId = ref _keys.Current;

                return new ActorRef<T1>(
                    actorId,
                    ref _component1.GetValue(actorId));
            }
        }

        private ReadOnlySpan<ActorId>.Enumerator _keys;
        private readonly ValueAccessor<T1> _component1;

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(
            ReadOnlySpan<ActorId> keys,
            ValueAccessor<T1> component1)
        {
            _keys = keys.GetEnumerator();
            _component1 = component1;
        }

        public bool MoveNext()
        {
            return _keys.MoveNext();
        }
    }
}
