using Hexecs.Actors.Components;

namespace Hexecs.Actors;

public sealed partial class ActorConstraint
{
    [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
    public sealed class Builder : IEquatable<ActorConstraint>
    {
        public readonly ActorContext Context;

        private int _hash;
        private int _length;
        private Subscription[] _subscriptions;

        internal Builder(ActorContext context)
        {
            Context = context;

            _length = 0;
            _subscriptions = ArrayPool<Subscription>.Shared.Rent(4);
        }

        public ActorConstraint Build()
        {
            var subscriptions = new Subscription[_length];
            Array.Copy(_subscriptions, subscriptions, _length);

            var instance = new ActorConstraint(Context.Id, _hash, subscriptions);

            ArrayPool<Subscription>.Shared.Return(_subscriptions, true);

            return instance;
        }

        public void Clear()
        {
            if (_subscriptions.Length > 0) ArrayPool<Subscription>.Shared.Return(_subscriptions);
            _length = 0;
        }

        public bool Equals(ActorConstraint? constraint)
        {
            if (constraint == null || constraint._hash != _hash) return false;

            var currentSubscriptions = _subscriptions.AsSpan(0, _length);
            var constraintSubscriptions = constraint._subscriptions;
            for (var i = 0; i < currentSubscriptions.Length; i++)
            {
                if (!_subscriptions[i].Equals(constraintSubscriptions[i])) return false;
            }

            return true;
        }

        public Builder Exclude<T>() where T : struct, IActorComponent
        {
            var pool = Context.GetOrCreateComponentPool<T>();

            AddSubscription<T>(false, pool, id => !pool.Has(id));

            return this;
        }

        public Builder Include<T>() where T : struct, IActorComponent
        {
            var pool = Context.GetOrCreateComponentPool<T>();

            AddSubscription<T>(true, pool, pool.Has);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddSubscription<T>(bool include, IActorComponentPool pool, Func<uint, bool> check)
            where T : struct, IActorComponent
        {
            var id = ActorComponentType<T>.Id;

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var exists in _subscriptions.AsSpan(0, _length))
            {
                if (exists.ComponentId == id) ActorError.ConstraintExists<T>();
            }

            ArrayUtils.Insert(
                ref _subscriptions,
                ArrayPool<Subscription>.Shared,
                _length,
                new Subscription(include, pool, check));

            _length++;

            Array.Sort(_subscriptions, 0, _length);

            var hash = Context.Id;

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var subscription in _subscriptions)
            {
                hash = HashCode.Combine(hash, subscription.GetHashCode());
            }

            _hash = hash;
        }
    }
}