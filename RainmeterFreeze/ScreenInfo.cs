using System.Runtime.InteropServices;
using RainmeterFreeze.Native;
using RainmeterFreeze.Native.Structures;

namespace RainmeterFreeze;

/// <summary>
/// Provides information about the screen and the windows that are displayed on it.
/// </summary>
public static class ScreenInfo
{
    /// <summary>
    /// Checks if the foreground window is in full-screen mode.
    /// </summary>
    public static bool IsForegroundFullScreen()
    {
        var rect = new RECT();
        nint hWnd = User32.GetForegroundWindow();

        if (!User32.GetWindowRect(new HandleRef(null, hWnd), ref rect))
            return false;

        nint monitor = User32.MonitorFromWindow(hWnd, MonitorFromWindowFlags.DefaultToNearest);
        if (monitor == 0)
            return false;

        var monitorInfo = new MonitorInfo();
        if (!User32.GetMonitorInfo(monitor, ref monitorInfo))
            return false;

        var monitorRect = monitorInfo.Monitor;

        return (
            rect.Left == monitorRect.Left &&
            rect.Top == monitorRect.Top &&
            rect.Right == monitorRect.Right &&
            rect.Bottom == monitorRect.Bottom
        );
    }
}
