namespace Hexecs.Utils;

public static class Error
{
    [DoesNotReturn]
    public static void Raise(string message) => throw new Exception(message);

    [DoesNotReturn]
    public static T Raise<T>(string message) => throw new Exception(message);
}