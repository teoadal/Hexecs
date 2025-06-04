using System.Collections.Concurrent;

namespace Hexecs.Configurations;

public sealed class ConfigurationService
{
    public static ConfigurationService Empty => new([], []);

    private readonly IConfigurationSource[] _sources;
    private readonly ConcurrentDictionary<string, object?> _values;

    internal ConfigurationService(
        IConfigurationSource[] sources,
        ConcurrentDictionary<string, object?> values)
    {
        _sources = sources;
        _values = values;
    }

    public T GetRequiredValue<T>(string key)
    {
        var result = GetValue<T>(key);
        if (result == null) ConfigurationError.KeyNotFound(key);

        return result;
    }

    public T? GetValue<T>(string key) => (T?)_values.GetOrAdd(key, static (k, sources) =>
    {
        foreach (var source in sources)
        {
            if (source.TryGetValue<T>(k, out var exists)) return exists;
        }

        return null;
    }, _sources);
}