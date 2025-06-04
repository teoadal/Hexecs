namespace Hexecs.Utils;

internal static class DelegateUtils<T>
{
    public static readonly Func<T, bool> AlwaysTrue = static _ => true;
    
    public static readonly Action<T> EmptyAction = static _ => {};
}