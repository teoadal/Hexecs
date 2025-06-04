namespace Hexecs.Values;

#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
internal sealed class ValueTable<TKey, TValue> : Dictionary<TKey, TValue>, IValueTable<TKey, TValue>
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
    where TKey : notnull
    where TValue : notnull
{
    public string Name { get; }

    // ReSharper disable once ConvertToPrimaryConstructor
    public ValueTable(string name, int capacity = 16, IEqualityComparer<TKey>? comparer = null)
        : base(capacity, comparer)
    {
        Name = name;
    }

    public bool Has(TKey key) => ContainsKey(key);

    public TValue Get(TKey key) => this[key];

    public bool Has(TKey key, TValue value)
    {
        return TryGetValue(key, out var actual) && EqualityComparer<TValue>.Default.Equals(actual, value);
    }

    public void Set(TKey key, TValue value) => this[key] = value;

    public bool TryGet(TKey key, out TValue value) => TryGetValue(key, out value!);
}