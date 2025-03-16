using System.Text.Json;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using RainmeterFreeze.Native;
using RainmeterFreeze.Enumerations;
using RainmeterFreeze.Native.Enumerations;
using RainmeterFreeze.Native.Structures;

namespace RainmeterFreeze;

static class Program
{
    static HookManager hooks = null!;
    static ControlTrayIcon trayIcon = null!;
    static Process? rainmeterProcess;
    static bool isFrozen;

    /// <summary>
    /// The path to the application's data folder.
    /// </summary>
    public readonly static string DataFolderPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RainmeterFreeze"
    );
    
    readonly static string StacktraceLogPath = Path.Combine(DataFolderPath, "stacktrace.log");

    /// <summary>
    /// The configuration currently used by the application.
    /// </summary>
    public static AppConfiguration Configuration = null!;

    /// <summary>
    /// Occurs when the freeze algorithm of this <see cref="RainmeterFreezeAppContext"/> has changed.
    /// </summary>
    public static event Action? FreezeAlgorithmChanged;

    /// <summary>
    /// Occurs when the freeze mode of this <see cref="RainmeterFreezeAppContext"/> has changed.
    /// </summary>
    public static event Action? FreezeModeChanged;

    static void Main()
    {
        AppDomain.CurrentDomain.UnhandledException += GlobalExceptionHandler;

        Directory.CreateDirectory(DataFolderPath);

        PerformAlreadyRunningCheck();

        try {
            Configuration = AppConfiguration.Load();
        } catch (JsonException ex) {
            var dr = User32.MessageBox(
                default,
                $"""
                The RainmeterFreeze configuration file is corrupted and cannot be parsed.
                
                {ex.Message}
                
                Do you wish to reset the configuration file and continue, or cancel and exit the application?
                """,
                "RainmeterFreeze Configuration Error",
                MessageBoxType.OKCancel | MessageBoxType.IconError
            );

            if (dr == DialogResult.Cancel) {
                Environment.Exit(1);
                return;
            }

            Configuration = new AppConfiguration();
            Configuration.Save();
        }

        hooks = new();
        hooks.ForegroundChanged += static () => {
            if (!RefreshRainmeterProcess())
                return;

            if (CheckIfShouldFreeze()) {
                Freeze();
            } else {
                Unfreeze();
            }
        };

        hooks.MouseEvent += static (ref MouseLowLevelHookStruct evPtr) => {
            // This only concerns the "not on desktop" mode. When the mode is set to "maximized"
            // or "full-screen", the user can't interact with a frozen Rainmeter skin (because
            // it'd be obstructed by a full-screen window).
            if (!isFrozen || Configuration.FreezeAlgorithm != FreezeAlgorithm.NotOnDesktop || !IsRainmeterRunning)
                return;

            // If the foreground window is maximized, or if it's in full-screen mode,
            // there is no possible way the user could've clicked a skin.
            if (User32.IsZoomed(User32.GetForegroundWindow()) || ScreenInfo.IsForegroundFullScreen())
                return;

            var ev = evPtr; // dereference ref variable from unmanaged memory

            // Rainmeter is frozen right now, and we might have clicked on one of
            // its skins. If this is the case, spawn a thread to analyze where we
            // clicked and whether this click intersects any Rainmeter skin.
            new Thread(() => {
                bool skinClicked = false;

                foreach (ProcessThread thread in rainmeterProcess.Threads) {
                    User32.EnumThreadWindows(thread.Id, (hwnd, _) => {
                        var rect = new Rect();
                        User32.GetWindowRect(hwnd, ref rect);

                        if (rect.Contains(ev.Pt)) {
                            // Play it safe - assume we clicked on the skin.
                            skinClicked = true;
                            return false; // stop iterating
                        }

                        return true; // continue iterating
                    }, default);

                    if (skinClicked) {
                        break;
                    }
                }

                if (skinClicked) {
                    Unfreeze();
                }
            }).Start();
        };

        trayIcon = new ControlTrayIcon();
    }

    static void PerformAlreadyRunningCheck()
    {
        Process thisProcess = Process.GetCurrentProcess();

        // Process.MainModule will throw an exception if we try to access a process like System (PID 4)!
        if (Process.GetProcesses().Any(x =>
            x.Id != thisProcess.Id &&
            x.ProcessName == thisProcess.ProcessName &&
            x.MainModule?.FileName == thisProcess.MainModule?.FileName
        )) {
            User32.MessageBox(
                default,
                "An existing instance of RainmeterFreeze is already running.",
                "RainmeterFreeze",
                MessageBoxType.OK | MessageBoxType.IconInformation
            );

            Environment.Exit(2);
        }
    }

    /// <summary>
    /// Sets the algorithm to use when determining when to freeze Rainmeter.
    /// </summary>
    /// <param name="algorithm">The target algorithm to use.</param>
    public static void SetFreezeAlgorithm(FreezeAlgorithm algorithm)
    {
        Configuration.FreezeAlgorithm = algorithm;
        FreezeAlgorithmChanged?.Invoke();
        Configuration.Save();
    }

    /// <summary>
    /// Sets the mode to use when freezing or un-freezing Rainmeter.
    /// </summary>
    /// <param name="mode">The target mode to use.</param>
    public static void SetFreezeMode(FreezeMode mode)
    {
        if (RefreshRainmeterProcess()) {
            Unfreeze();
        }

        Configuration.FreezeMode = mode;
        FreezeModeChanged?.Invoke();
        Configuration.Save();

        if (CheckIfShouldFreeze()) {
            Freeze();
        }
    }

    static bool CheckIfShouldFreeze()
    {
        nint hwnd = User32.GetForegroundWindow();

        if (
            hwnd == User32.FindWindow("Shell_TrayWnd", null) ||
            hwnd == User32.GetDesktopWindow() ||
            hwnd == User32.GetShellWindow()
        ) {
            return false;
        }

        Span<char> className = stackalloc char[32];
        User32.GetClassName(hwnd, className);

        switch (className[..className.IndexOf('\0')]) {
            case "WorkerW":
            case "SysListView32":
            case "SHELLDLL_DefView":
            // Do not freeze when interacting with the system tray
            case "NotifyIconOverflowWindow":
            case "RainmeterTrayClass":
                return false;
        }

        // Finally, check if the given window handle belongs to the
        // Rainmeter process.
        User32.GetWindowThreadProcessId(hwnd, out uint pid);

        if (Process.GetProcessById((int)pid).ProcessName == "Rainmeter") {
            return false;
        }

        // We have determined that the active window is not a Rainmeter or
        // the desktop window - if our algorithm is "Not on desktop", then
        // we simply return true
        if (Configuration.FreezeAlgorithm == FreezeAlgorithm.NotOnDesktop)
            return true;

        // ...however, with the other algorithms, we have to perform additional
        // window checks.
        if (Configuration.FreezeAlgorithm == FreezeAlgorithm.Maximized) {
            // Wait for any maximization animation to end.
            // IsZoomed returns false when the maximize animation is playing,
            // and when not given enough time for Rainmeter to process that
            // its not in the foreground anymore, the widgets can stay on top.
            Thread.Sleep(100);
            return User32.IsZoomed(hwnd);
        }

        if (Configuration.FreezeAlgorithm == FreezeAlgorithm.FullScreen)
            return ScreenInfo.IsForegroundFullScreen();

        throw new NotImplementedException($"Unknown algorithm '{Configuration.FreezeAlgorithm}'");
    }

    static void Freeze()
    {
        if (!RefreshRainmeterProcess())
            return;

        switch (Configuration.FreezeMode) {
            case FreezeMode.Suspend: {
                ProcessManagement.SuspendProcess(rainmeterProcess.Id);
                break;
            }
            case FreezeMode.LowPriority: {
                rainmeterProcess.PriorityClass = ProcessPriorityClass.BelowNormal;
                break;
            }
            default: throw new UnreachableException();
        }

        isFrozen = true;
    }

    static void Unfreeze()
    {
        if (!RefreshRainmeterProcess())
            return;

        switch (Configuration.FreezeMode) {
            case FreezeMode.Suspend: {
                ProcessManagement.ResumeProcess(rainmeterProcess.Id);
                break;
            }
            case FreezeMode.LowPriority: {
                rainmeterProcess.PriorityClass = ProcessPriorityClass.Normal;
                break;
            }
            default: throw new UnreachableException();
        }

        isFrozen = false;
    }

    [MemberNotNullWhen(true, nameof(rainmeterProcess))]
    static bool IsRainmeterRunning => rainmeterProcess != null && !rainmeterProcess.HasExited;

    /// <summary>
    /// Refreshes the stored Rainmeter process ID, if necessary.
    /// </summary>
    /// <returns>'true' if the process ID was successfully fetched, or if a refresh is not required at the given time.</returns>
    [MemberNotNullWhen(true, nameof(rainmeterProcess))]
    static bool RefreshRainmeterProcess()
    {
        if (IsRainmeterRunning)
            return true;

        var processes = Process.GetProcessesByName("Rainmeter");

        if (processes.Length == 0) {
            // We couldn't find any Rainmeter processes...
            rainmeterProcess = null;
            return false;
        }

        // At least one Rainmeter process has been found
        rainmeterProcess = processes[0];
        return true;
    }

    /// <summary>
    /// Exits the application.
    /// </summary>
    public static void Exit()
    {
        trayIcon.Icon.Dispose();
        hooks.Dispose();

        if (IsRainmeterRunning) {
            ProcessManagement.ResumeProcess(rainmeterProcess.Id);
        }

        Environment.Exit(0);
    }

    static void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs e)
    {
        if (!e.IsTerminating) return;

        if(e.ExceptionObject is Exception ex) {
            try {
                File.WriteAllText(
                    StacktraceLogPath,
                    $"""
                    A fatal exception has been thrown and the application cannot continue.
                    
                    {ex.GetType()}: {ex.Message}
                    {ex.StackTrace}
                    """
                );
            }
            catch (Exception dumpErr) {
                User32.MessageBox(
                    default,
                    $"""
                    A fatal exception occured which could not be dumped to a file.
                    
                    {ex.GetType()}: {ex.Message}
                    {ex.StackTrace}
                    
                    The following exception occurred while attempting to dump the information to a file:
                    
                    {dumpErr.GetType()}: {dumpErr.Message}
                    {dumpErr.StackTrace}
                    """,
                    "RainmeterFreeze",
                    MessageBoxType.OK | MessageBoxType.IconError
                );
            }
        } else {
            try {
                File.WriteAllText(
                    StacktraceLogPath,
                    $"""
                    An unknown fatal exception has been thrown and the application cannot continue.
                    
                    Exception type: {e.ExceptionObject.GetType()}
                    Exception: {e.ExceptionObject}
                    """
                );
            } catch (Exception dumpErr) {
                User32.MessageBox(
                    default,
                    $"""
                    A fatal exception occured which could not be dumped to a file.
                    
                    {e.ExceptionObject.GetType()}: {e.ExceptionObject}
                    
                    The following exception occurred while attempting to dump the information to a file:
                    
                    {dumpErr.GetType()}: {dumpErr.Message}
                    {dumpErr.StackTrace}
                    """,
                    "RainmeterFreeze",
                    MessageBoxType.OK | MessageBoxType.IconError
                );
            }
        }
    }
}
