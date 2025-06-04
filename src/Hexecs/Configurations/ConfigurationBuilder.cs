using System.Collections.Concurrent;

namespace Hexecs.Configurations;

public sealed class ConfigurationBuilder
{
    private readonly List<IConfigurationSource> _sources = [];
    private readonly ConcurrentDictionary<string, object?> _values = new();

    internal ConfigurationBuilder()
    {
    }

    internal ConfigurationService Build()
    {
        foreach (var source in _sources)
        {
            source.Load();
        }

        return new ConfigurationService(_sources.ToArray(), _values);
    }

    public ConfigurationBuilder UseSource(IConfigurationSource source)
    {
        _sources.Add(source);
        return this;
    }

    public ConfigurationBuilder UseValue<T>(string key, T? value)
    {
        _values[key] = value;
        return this;
    }
}