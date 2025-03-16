using System.Diagnostics;
using System.Runtime.CompilerServices;
using RainmeterFreeze.Native;
using RainmeterFreeze.Native.Enumerations;
using RainmeterFreeze.Native.Structures;

namespace RainmeterFreeze;

/// <summary>
/// Manages global Windows message hooks.
/// </summary>
class HookManager : IDisposable
{
    User32.WinEventDelegate? eventHandler;
    nint wndChangeHook;
    nint wndMinimizeHook;
    nint wndDestroyHook;
    nint mouseHook;
    readonly Thread msgLoop;
    
    public delegate void MouseEventHandler(ref MouseLowLevelHookStruct ev);

    public event Action? ForegroundChanged;
    public event MouseEventHandler? MouseEvent;

    public unsafe HookManager()
    {
        msgLoop = new Thread(() => {
            eventHandler = new User32.WinEventDelegate(HandleEvent);

            wndChangeHook = HookSingle(User32.EventSystemForeground);
            wndMinimizeHook = HookSingle(User32.EventSystemMinimizeStart);
            wndDestroyHook = HookSingle(User32.EventObjectDestroy);

            mouseHook = User32.SetWindowsHookEx(
                WindowsHookType.MouseLowLevel,
                (nCode, wParam, lParam) => {
                    if (wParam != User32.WmMouseMove && nCode >= 0) {
                        var ev = (MouseLowLevelHookStruct*)lParam;
                        if (ev != null) {
                            MouseEvent?.Invoke(ref Unsafe.AsRef<MouseLowLevelHookStruct>(ev));
                        }
                    }

                    return User32.CallNextHookEx(mouseHook, nCode, wParam, lParam);
                },
                Kernel32.GetModuleHandle(null),
                0 // listen globally
            );

            while (true) {
                var msg = new Msg();
                var result = User32.GetMessage(ref msg, default, 0, 0);

                if (result is 0 or -1)
                    break;

                User32.TranslateMessage(ref msg);
                User32.DispatchMessage(ref msg);
            }
        });

        msgLoop.Start();
    }

    nint HookSingle(uint type)
        => User32.SetWinEventHook(
            type, type,
            0,
            eventHandler ?? throw new("Attempted to call HookSingle before the event handler delegate was allocated."),
            0, 0,
            User32.WinEventOutOfContext
        );

    void HandleEvent(nint hWinEventHook, uint eventType, nint hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
    {
        switch (eventType) {
            case User32.EventSystemForeground: {
                ForegroundChanged?.Invoke();
                break;
            }
            case User32.EventSystemMinimizeStart: {
                if (hwnd == User32.GetForegroundWindow()) {
                    ForegroundChanged?.Invoke();
                }

                break;
            }
            case User32.EventObjectDestroy: {
                if (idObject == User32.ObjIdWindow && hwnd == User32.GetForegroundWindow()) {
                    ForegroundChanged?.Invoke();
                }

                break;
            }
        }
    }

    public void Dispose()
    {
        User32.UnhookWinEvent(wndDestroyHook);
        User32.UnhookWinEvent(wndMinimizeHook);
        User32.UnhookWinEvent(wndChangeHook);

        User32.UnhookWindowsHookEx(mouseHook);
    }
}