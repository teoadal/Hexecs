using System.Runtime.CompilerServices;

namespace Hexecs.Benchmarks.Map.Utils;

public struct CameraViewport : IEquatable<CameraViewport>
{
    public int Left;
    public int Right;
    public int Top;
    public int Bottom;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Hidden(int x, int y, int width, int height) => !Visible(x, y, width, height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Visible(int x, int y, int width, int height) =>
        x < Right &&
        Left < x + width
        && y < Bottom &&
        Top < y + height;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(CameraViewport other) => Left == other.Left &&
                                                Right == other.Right &&
                                                Top == other.Top &&
                                                Bottom == other.Bottom;

    public override bool Equals(object? obj) => obj is CameraViewport other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Left, Right, Top, Bottom);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in CameraViewport left, in CameraViewport right) => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in CameraViewport left, in CameraViewport right) => !left.Equals(right);
}