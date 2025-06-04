namespace Hexecs.Configurations;

public interface IConfigurationSource
{
    void Load();

    bool TryGetValue<T>(string key, out T? value);
}