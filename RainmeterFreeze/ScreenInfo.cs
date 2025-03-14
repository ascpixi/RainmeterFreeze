using System.Windows.Forms;
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

        User32.GetWindowRect(new HandleRef(null, hWnd), ref rect);

        return (
            Screen.PrimaryScreen.Bounds.Width == (rect.Right - rect.Left) &&
            Screen.PrimaryScreen.Bounds.Height == (rect.Bottom - rect.Top)
        );
    }
}
