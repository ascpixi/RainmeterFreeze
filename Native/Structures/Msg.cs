using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
struct Msg
{
    public nint Hwnd;
    public uint Message;
    public nint WParam;
    public nuint LParam;
    public uint Time;
    public Point Pt;
    public uint LPrivate;
}