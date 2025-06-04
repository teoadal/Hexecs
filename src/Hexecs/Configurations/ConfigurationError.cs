namespace Hexecs.Configurations;

internal static class ConfigurationError
{
    [DoesNotReturn]
    public static void KeyNotFound(string key)
    {
        throw new KeyNotFoundException($"Configuration key '{key}' isn't found");
    }
}