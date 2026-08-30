using Hexecsm.Accessors;

namespace Hexecsm.Filters;

public sealed partial class Filter<T1, T2, T3>
{
    public ref struct Enumerator
    {
        public ActorRef<T1, T2, T3> Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ref readonly ActorId actorId = ref _keys.Current;

                return new ActorRef<T1, T2, T3>(
                    actorId,
                    ref _component1.GetValue(actorId),
                    ref _component2.GetValue(actorId),
                    ref _component3.GetValue(actorId));
            }
        }

        private ReadOnlySpan<ActorId>.Enumerator _keys;
        private readonly ValueAccessor<T1> _component1;
        private readonly ValueAccessor<T2> _component2;
        private readonly ValueAccessor<T3> _component3;

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(
            ReadOnlySpan<ActorId> keys,
            ValueAccessor<T1> component1,
            ValueAccessor<T2> component2,
            ValueAccessor<T3> component3)
        {
            _keys = keys.GetEnumerator();
            _component1 = component1;
            _component2 = component2;
            _component3 = component3;
        }

        public bool MoveNext()
        {
            return _keys.MoveNext();
        }
    }
}
