using System.Runtime.InteropServices;

namespace RainmeterFreeze.Native.Structures {
    /// <summary>
    /// The <see cref="RECT"/> structure defines a rectangle by the coordinates
    /// of its upper-left and lower-right corners.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT {
        /// <summary>
        /// Specifies the x-coordinate of the upper-left corner of the rectangle.
        /// </summary>
        internal int left;

        /// <summary>
        /// Specifies the y-coordinate of the upper-left corner of the rectangle.
        /// </summary>
        internal int top;

        /// <summary>
        /// Specifies the x-coordinate of the lower-right corner of the rectangle.
        /// </summary>
        internal int right;

        /// <summary>
        /// Specifies the y-coordinate of the lower-right corner of the rectangle.
        /// </summary>
        internal int bottom;
    }
}
