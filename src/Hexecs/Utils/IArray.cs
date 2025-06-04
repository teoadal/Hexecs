namespace Hexecs.Utils;

public interface IArray<T>
{
    int Length { get; }

    Span<T> AsSpan();

    T this[int index] { get; set; }
}