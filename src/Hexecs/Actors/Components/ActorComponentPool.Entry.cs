namespace Hexecs.Actors.Components;

internal sealed partial class ActorComponentPool<T>
{
    private struct Entry
    {
        public int Next;
        public uint Key;
        public T Value;
    }
}