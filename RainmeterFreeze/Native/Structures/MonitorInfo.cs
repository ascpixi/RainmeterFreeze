using System.Runtime.InteropServices;

namespace RainmeterFreeze.Native.Structures;

[StructLayout(LayoutKind.Sequential)]
struct MonitorInfo()
{
    public uint Size = (uint)Marshal.SizeOf<MonitorInfo>();
    public readonly RECT Monitor;
    public readonly RECT Work;
    public readonly MonitorInfoFlags Flags;
}

enum MonitorInfoFlags
{
    Primary = 0x00000001
}