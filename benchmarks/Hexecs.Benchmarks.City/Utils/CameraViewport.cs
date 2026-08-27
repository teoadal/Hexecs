using System.Runtime.CompilerServices;

namespace Hexecs.Benchmarks.Map.Utils;

public struct CameraViewport : IEquatable<CameraViewport>
{
    public int Left;
    public int Right;
    public int Top;
    public int Bottom;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Hidden(int x, int y, int width, int height)
    {
        return !Visible(x, y, width, height);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Visible(int x, int y, int width, int height)
    {
        return x < Right &&
            Left < x + width
            && y < Bottom &&
            Top < y + height;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(CameraViewport other)
    {
        return Left == other.Left &&
            Right == other.Right &&
            Top == other.Top &&
            Bottom == other.Bottom;
    }

    public override bool Equals(object? obj)
    {
        return obj is CameraViewport other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Left, Right, Top, Bottom);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in CameraViewport left, in CameraViewport right)
    {
        return left.Equals(right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in CameraViewport left, in CameraViewport right)
    {
        return !left.Equals(right);
    }
}