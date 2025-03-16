using System.Runtime.InteropServices;

namespace RainmeterFreeze.Native.Structures;

[StructLayout(LayoutKind.Sequential)]
readonly struct MouseLowLevelHookStruct
{
    /// <summary>
    /// The x- and y-coordinates of the cursor, in per-monitor-aware screen coordinates.
    /// </summary>
    public readonly Point Pt;

    public readonly uint MouseData;
    public readonly uint Flags;
    public readonly uint Time;
    public readonly nuint DwExtraInfo;
}