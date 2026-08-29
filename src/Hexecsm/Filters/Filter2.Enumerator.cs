using Hexecsm.Accessors;

namespace Hexecsm.Filters;

public sealed partial class Filter<T1, T2>
{
    public ref struct Enumerator
    {
        public ActorRef<T1, T2> Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ref readonly ActorId actorId = ref _keys.Current;

                return new ActorRef<T1, T2>(
                    actorId,
                    ref _component1.GetValue(actorId),
                    ref _component2.GetValue(actorId));
            }
        }

        private ReadOnlySpan<ActorId>.Enumerator _keys;
        private readonly ValueAccessor<T1> _component1;
        private readonly ValueAccessor<T2> _component2;

        internal Enumerator(
            ReadOnlySpan<ActorId> keys,
            ValueAccessor<T1> component1,
            ValueAccessor<T2> component2)
        {
            _keys = keys.GetEnumerator();
            _component1 = component1;
            _component2 = component2;
        }

        public bool MoveNext()
        {
            return _keys.MoveNext();
        }
    }
}
