namespace Hexecs.Actors;

public sealed partial class ActorFilter<T1, T2, T3>
{
    private sealed class DebugProxy(ActorFilter<T1, T2, T3> filter)
    {
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public IEnumerable<Actor> Actors
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => filter._dictionary.Keys.Select(key => new Actor(filter.Context, key));
        }
    }
}