﻿using System;
using System.Text;
using System.Runtime.InteropServices;
using RainmeterFreeze.Native.Structures;
using RainmeterFreeze.Native.Enumerations;

using DialogResult = RainmeterFreeze.Native.Enumerations.DialogResult;

namespace RainmeterFreeze.Native;

/// <summary>
/// Provides native methods from the USER32 Dynamic Link Library.
/// </summary>
internal static class User32
{
    /// <summary>
    /// An application-defined callback (or hook) function that the system calls
    /// in response to events generated by an accessible object. The hook
    /// function processes the event notifications as required. Clients install 
    /// the hook function and request specific types of event notifications by
    /// calling SetWinEventHook.
    /// </summary>
    /// <param name="hWinEventHook">Handle to an event hook function. This value is returned by SetWinEventHook when the hook function is installed and is specific to each instance of the hook function.</param>
    /// <param name="eventType">Specifies the event that occurred. This value is one of the event constants.</param>
    /// <param name="hwnd">Handle to the window that generates the event, or NULL if no window is associated with the event. For example, the mouse pointer is not associated with a window.</param>
    /// <param name="idObject">Identifies the object associated with the event. This is one of the object identifiers or a custom object ID.</param>
    /// <param name="idChild">Identifies whether the event was triggered by an object or a child element of the object. If this value is CHILDID_SELF, the event was triggered by the object; otherwise, this value is the child ID of the element that triggered the event.</param>
    /// <param name="dwmsEventTime">Specifies the time, in milliseconds, that the event was generated.</param>
    internal delegate void WinEventDelegate(
        nint hWinEventHook,
        uint eventType,
        nint hwnd,
        int idObject,
        int idChild,
        uint dwEventThread,
        uint dwmsEventTime
    );

    /// <summary>
    /// The callback function is not mapped into the address space of the process that generates the event. Because the hook function is called across process boundaries, the system must queue events. Although this method is asynchronous, events are guaranteed to be in sequential order. 
    /// </summary>
    internal const uint WinEventOutOfContext = 0;

    /// <summary>
    /// The foreground window has changed. The system sends this event even if the foreground window has changed to another window in the same thread. Server applications never send this event.
    /// For this event, the WinEventProc callback function's hwnd parameter is the handle to the window that is in the foreground, the idObject parameter is OBJID_WINDOW, and the idChild parameter is CHILDID_SELF.
    /// </summary>
    internal const uint EventSystemForeground = 3;

    /// <summary>
    /// Sets an event hook function for a range of events.
    /// </summary>
    /// <param name="eventMin">Specifies the event constant for the lowest event value in the range of events that are handled by the hook function.</param>
    /// <param name="eventMax">Specifies the event constant for the highest event value in the range of events that are handled by the hook function.</param>
    /// <param name="hmodWinEventProc">Handle to the DLL that contains the hook function at lpfnWinEventProc, if the WINEVENT_INCONTEXT flag is specified in the dwFlags parameter. If the hook function is not located in a DLL, or if the WINEVENT_OUTOFCONTEXT flag is specified, this parameter is NULL.</param>
    /// <param name="lpfnWinEventProc">Pointer to the event hook function.</param>
    /// <param name="idProcess">Specifies the ID of the process from which the hook function receives events. Specify zero (0) to receive events from all processes on the current desktop.</param>
    /// <param name="idThread">Specifies the ID of the thread from which the hook function receives events. If this parameter is zero, the hook function is associated with all existing threads on the current desktop.</param>
    /// <param name="dwFlags">Flag values that specify the location of the hook function and of the events to be skipped.</param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    internal static extern nint SetWinEventHook(
        uint eventMin,
        uint eventMax,
        nint hmodWinEventProc,
        WinEventDelegate lpfnWinEventProc,
        uint idProcess,
        uint idThread,
        uint dwFlags
    );

    /// <summary>
    /// Removes an event hook function created by a previous call to SetWinEventHook.
    /// </summary>
    /// <param name="hWinEventHook">Handle to the event hook returned in the previous call to SetWinEventHook.</param>
    /// <returns>If successful, returns TRUE; otherwise, returns FALSE.</returns>
    [DllImport("user32.dll")]
    internal static extern bool UnhookWinEvent(nint hWinEventHook);

    /// <summary>
    /// Retrieves a handle to the foreground window (the window with which the
    /// user is currently working). The system assigns a slightly higher priority
    /// to the thread that creates the foreground window than it does to other threads.
    /// </summary>
    /// <returns>The return value is a handle to the foreground window. The foreground window can be NULL in certain circumstances, such as when a window is losing activation.</returns>
    [DllImport("user32.dll")]
    internal static extern nint GetForegroundWindow();

    /// <summary>
    /// Retrieves a handle to the desktop window. The desktop window
    /// covers the entire screen. The desktop window is the area on top
    /// of which other windows are painted.
    /// </summary>
    /// <returns>The return value is a handle to the desktop window.</returns>
    [DllImport("user32.dll")]
    internal static extern nint GetDesktopWindow();

    /// <summary>
    /// Retrieves a handle to the Shell's desktop window.
    /// </summary>
    /// <returns>The return value is the handle of the Shell's desktop window. If no Shell process is present, the return value is NULL.</returns>
    [DllImport("user32.dll")]
    internal static extern nint GetShellWindow();

    /// <summary>
    /// Retrieves a handle to the top-level window whose class name and window name match the specified strings. This function does not search child windows.
    /// This function does not perform a case-sensitive search.
    /// </summary>
    /// <param name="lpClassName">The class name or a class atom created by a previous call to the RegisterClass or RegisterClassEx function. The atom must be in the low-order word of lpClassName; the high-order word must be zero.</param>
    /// <param name="lpWindowName">The window name (the window's title). If this parameter is NULL, all window names match.</param>
    /// <returns>If the function succeeds, the return value is a handle to the window that has the specified class name and window name. If the function fails, the return value is NULL.</returns>
    [DllImport("user32.dll")]
    internal static extern nint FindWindow(
        string lpClassName,
        string? lpWindowName
    );

    /// <summary>
    /// Retrieves the name of the class to which the specified window belongs.
    /// </summary>
    /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs.</param>
    /// <param name="lpClassName">The class name string.</param>
    /// <param name="nMaxCount">The length of the lpClassName buffer, in characters. The buffer must be large enough to include the terminating null character; otherwise, the class name string is truncated to nMaxCount-1 characters.</param>
    /// <returns>If the function succeeds, the return value is the number of characters copied to the buffer, not including the terminating null character. If the function fails, the return value is zero.</returns>
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    internal static extern int GetClassName(
        nint hWnd,
        StringBuilder lpClassName,
        int nMaxCount
    );

    /// <summary>
    /// Retrieves the identifier of the thread that created the specified
    /// window and, optionally, the identifier of the process that created
    /// the window.
    /// </summary>
    /// <param name="hWnd">A handle to the window.</param>
    /// <param name="processId">A pointer to a variable that receives the process identifier. If this parameter is not NULL, GetWindowThreadProcessId copies the identifier of the process to the variable; otherwise, it does not.</param>
    /// <returns>The return value is the identifier of the thread that created the window.</returns>
    [DllImport("user32.dll", SetLastError = true)]
    internal static extern uint GetWindowThreadProcessId(
        nint hWnd,
        out uint processId
    );

    /// <summary>
    /// Determines whether a window is maximized.
    /// </summary>
    /// <param name="hWnd">A handle to the window to be tested.</param>
    [DllImport("user32.dll")]
    internal static extern bool IsZoomed(nint hWnd);

    /// <summary>
    /// Retrieves the dimensions of the bounding rectangle of the specified
    /// window. The dimensions are given in screen coordinates that are
    /// relative to the upper-left corner of the screen.
    /// </summary>
    /// <param name="hWnd">A handle to the window.</param>
    /// <param name="rect">A pointer to a RECT structure that receives the screen coordinates of the upper-left and lower-right corners of the window.</param>
    /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero.</returns>
    [DllImport("user32.dll")]
    internal static extern bool GetWindowRect(
        HandleRef hWnd,
        [In, Out] ref RECT rect
    );

    [DllImport("user32.dll")]
    internal static extern DialogResult MessageBox(
        nint hWnd,
        string lpText,
        string lpCaption,
        MessageBoxType type
    );

    [DllImport("user32.dll")]
    internal static extern nint MonitorFromWindow(
        nint hwnd,
        MonitorFromWindowFlags dwFlags
    );

    [DllImport("user32.dll")]
    internal static extern bool GetMonitorInfo(
        nint hMonitor,
        ref MonitorInfo lpmi
    );
}

enum MonitorFromWindowFlags
{
    DefaultToNull,
    DefaultToPrimary,
    DefaultToNearest,
}
