using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using RainmeterFreeze.Native;
using RainmeterFreeze.Native.Structures;

namespace RainmeterFreeze {
    /// <summary>
    /// Provides information about the screen and the windows that are displayed on it.
    /// </summary>
    public static class ScreenInfo {
        /// <summary>
        /// Checks if the foreground window is in full-screen mode.
        /// </summary>
        public static bool IsForegroundFullScreen()
        {
            RECT rect = new RECT();
            IntPtr hWnd = User32.GetForegroundWindow();

            User32.GetWindowRect(new HandleRef(null, hWnd), ref rect);

            if (Screen.PrimaryScreen.Bounds.Width == (rect.right - rect.left) && Screen.PrimaryScreen.Bounds.Height == (rect.bottom - rect.top)) {
                return true;
            }
            else {
                return false;
            }
        }
    }
}
