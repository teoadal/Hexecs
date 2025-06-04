using Hexecs.Attributes;

namespace Hexecs.Utils;

public sealed class OrderComparer<T> : IComparer<T>
    where T : class
{
    public static IComparer<T> CreateInstance() => new OrderComparer<T>();

    public int Compare(T? x, T? y)
    {
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (x == null && y == null) return 0;
        if (x == null) return -1;
        if (y == null) return 1;

        var xOrder = x is IHaveOrder xHasOrder
            ? xHasOrder.Order
            : OrderAttribute.TryGetValue(x.GetType());

        var yOrder = y is IHaveOrder yHasOrder
            ? yHasOrder.Order
            : OrderAttribute.TryGetValue(y.GetType());

        return xOrder.CompareTo(yOrder);
    }
}