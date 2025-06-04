using System.Drawing;

namespace Hexecs.Utils;

[DebuggerDisplay("X = {X}, Y = {Y}")]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly struct Position2D(int x, int y) : IComparable<Position2D>, IEquatable<Position2D>, IEquatable<Point>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position2D Add(in Point pt, in Size sz)
    {
        return new Position2D(unchecked(pt.X + sz.Width), unchecked(pt.Y + sz.Height));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position2D Add(in Position2D pt, in Size sz)
    {
        return new Position2D(unchecked(pt.X + sz.Width), unchecked(pt.Y + sz.Height));
    }

    public static readonly IEqualityComparer<Position2D> Comparer = new PositionComparer();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position2D Subtract(in Point pt, in Size sz)
    {
        return new Position2D(unchecked(pt.X - sz.Width), unchecked(pt.Y - sz.Height));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position2D Subtract(in Position2D pt, in Size sz)
    {
        return new Position2D(unchecked(pt.X - sz.Width), unchecked(pt.Y - sz.Height));
    }

    public readonly int X = x;
    public readonly int Y = y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point AsPoint() => new(X, Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PointF AsPointF() => new(X, Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Size AsSize() => new(X, Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(Position2D other)
    {
        var xComparison = X.CompareTo(other.X);
        return xComparison == 0
            ? Y.CompareTo(other.Y)
            : xComparison;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out int x, out int y)
    {
        x = X;
        y = Y;
    }

    #region Offset

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Position2D Offset(int dx, int dy) => new(X + dx, Y + dy);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Position2D Offset(in Position2D p) => Offset(p.X, p.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Position2D Offset(in Point p) => Offset(p.X, p.Y);

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position2D operator +(in Position2D pt, in Size sz) => Add(in pt, in sz);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position2D operator -(in Position2D pt, in Size sz) => Subtract(in pt, in sz);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator PointF(in Position2D p) => new(p.X, p.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Size(in Position2D p) => new(p.X, p.Y);

    #region Equality

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Point other) => X == other.X && Y == other.Y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Position2D other) => X == other.X && Y == other.Y;

    public override bool Equals(object? obj) => obj is Position2D position && Equals(position);

    public override int GetHashCode() => HashCode.Combine(X, Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in Position2D left, in Position2D right) => left.X == right.X && left.Y == right.Y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in Position2D left, in Position2D right) => left.X != right.X || left.Y != right.Y;

    #endregion

    private sealed class PositionComparer : IEqualityComparer<Position2D>, IAlternateEqualityComparer<Point, Position2D>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Position2D Create(Point alternate) => new(alternate.X, alternate.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Position2D x, Position2D y) => x.X == y.X && x.Y == y.Y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Point alternate, Position2D other) => alternate.X == other.X && alternate.Y == other.Y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetHashCode(Position2D obj) => HashCode.Combine(obj.X, obj.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetHashCode(Point alternate) => HashCode.Combine(alternate.X, alternate.Y);
    }
}