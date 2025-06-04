namespace Hexecs.Actors;

public sealed partial class ActorFilter<T1>
{
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly struct Entry(int index1)
    {
        public readonly int Index1 = index1;
    }
}