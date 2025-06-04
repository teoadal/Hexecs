namespace Hexecs.Actors;

public sealed partial class ActorFilter<T1, T2>
{
    private sealed class DebugProxy(ActorFilter<T1, T2> filter)
    {
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public IEnumerable<Actor> Actors
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => filter.ToArray();
        }
    }
}