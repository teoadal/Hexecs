namespace Hexecs.Values;

public sealed class ValueServiceBuilder
{
    private readonly List<IValueTable> _tables = [];

    internal ValueServiceBuilder()
    {
    }

    public ValueServiceBuilder AddTable<TKey, TValue>(IValueTable<TKey, TValue> table)
        where TKey : notnull
        where TValue : notnull
    {
        EnsureTableNotExists(table.Name);

        _tables.Add(table);

        return this;
    }

    internal ValueService Build()
    {
        var result = new ValueService(_tables);

        _tables.Clear();

        return result;
    }

    public ValueServiceBuilder CreateTable<TKey, TValue>(
        string tableName,
        int capacity = 16,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
        where TValue : notnull
    {
        EnsureTableNotExists(tableName);

        IValueTable? table = null;
        if (comparer == null)
        {
            if (typeof(TKey) == typeof(Type))
            {
                table = new ValueTable<Type, TValue>(tableName, capacity, ReferenceComparer<Type>.Instance);
            }
            else if (typeof(TKey) == typeof(string))
            {
                table = new ValueTable<string, TValue>(tableName, capacity, ReferenceComparer<string>.Instance);
            }
        }

        _tables.Add(table ?? new ValueTable<TKey, TValue>(tableName, capacity, comparer));

        return this;
    }

    private void EnsureTableNotExists(string name)
    {
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var table in _tables)
        {
            if (table.Name == name)
            {
                ValueError.TableAlreadyExists(name);
            }
        }
    }
}