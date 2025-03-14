using System.Runtime.InteropServices;

namespace RainmeterFreeze.Native.Structures;

/// <summary>
/// The <see cref="RECT"/> structure defines a rectangle by the coordinates
/// of its upper-left and lower-right corners.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct RECT
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
}
