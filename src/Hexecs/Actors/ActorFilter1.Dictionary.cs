namespace Hexecs.Actors;

public sealed partial class ActorFilter<T1>
{
    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _dictionary.Count;
    }

    private readonly Dictionary<uint, Entry> _dictionary;

    private void AddEntry(uint actorId, int index1)
    {
        ref var entry = ref CollectionsMarshal.GetValueRefOrAddDefault(_dictionary, actorId, out var exists);
        if (exists) ActorError.AlreadyExists(actorId);

        entry = new Entry(index1);

        Added?.Invoke(actorId);
    }

    private void ClearEntries() => _dictionary.Clear();

    private void RemoveEntry(uint actorId)
    {
        if (_dictionary.Remove(actorId)) Removed?.Invoke(actorId);
    }
}