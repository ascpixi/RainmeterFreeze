using System.Runtime.InteropServices;

namespace RainmeterFreeze.Native.Structures;

/// <summary>
/// The <see cref="Rect"/> structure defines a rectangle by the coordinates
/// of its upper-left and lower-right corners.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct Rect
{
    /// <summary>
    /// Specifies the x-coordinate of the upper-left corner of the rectangle.
    /// </summary>
    internal int Left;

    /// <summary>
    /// Specifies the y-coordinate of the upper-left corner of the rectangle.
    /// </summary>
    internal int Top;

    /// <summary>
    /// Specifies the x-coordinate of the lower-right corner of the rectangle.
    /// </summary>
    internal int Right;

    /// <summary>
    /// Specifies the y-coordinate of the lower-right corner of the rectangle.
    /// </summary>
    internal int Bottom;

    /// <summary>
    /// Returns 'true' when this rectangle contains the given point.
    /// </summary>
    public readonly bool Contains(int x, int y)
        => x >= Left && x <= Right && y >= Top && y <= Bottom;

    /// <summary>
    /// Equivalent to 'Contains(p.X, p.Y)'.
    /// </summary>
    public readonly bool Contains(Point p) => Contains(p.X, p.Y);

    public override readonly string ToString() => $"({Left}, {Top}, {Right}, {Bottom})";
}
