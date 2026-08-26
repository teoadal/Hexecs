namespace Hexecs.Utils;

internal static class ServiceProviderExtensions
{
    public static IEnumerable<T> GetServices<T>(this IServiceProvider? provider)
        where T: class
    {
        object? collection = provider?.GetService(typeof(IEnumerable<T>));
        return collection as IEnumerable<T> ?? [];
    }
}
