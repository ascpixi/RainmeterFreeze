using System.Text;
using System.Text.Json;
using System.Diagnostics;
using RainmeterFreeze.Native;
using RainmeterFreeze.Enumerations;
using RainmeterFreeze.Native.Enumerations;

namespace RainmeterFreeze;

static class Program
{
    static User32.WinEventDelegate windowChangeHandler = null!;
    static IntPtr windowChangeHookPtr;
    static ControlTrayIcon trayIcon = null!;
    static int rainmeterPid = -1; // the current process ID of Rainmeter

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

        windowChangeHandler = new User32.WinEventDelegate(HandleWindowChanged);
        windowChangeHookPtr = User32.SetWinEventHook(
            User32.EventSystemForeground,
            User32.EventSystemForeground,
            0,
            windowChangeHandler,
            0, 0,
            User32.WinEventOutOfContext
        );

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
        if (RefreshRainmeterPid()) {
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

        switch (className) {
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
        if (!RefreshRainmeterPid())
            return;

        switch (Configuration.FreezeMode) {
            case FreezeMode.Suspend: {
                ProcessManagement.SuspendProcess(rainmeterPid);
                break;
            }
            case FreezeMode.LowPriority: {
                Process.GetProcessById(rainmeterPid).PriorityClass = ProcessPriorityClass.BelowNormal;
                break;
            }
            default: throw new UnreachableException();
        }
    }

    static void Unfreeze()
    {
        if (!RefreshRainmeterPid())
            return;

        switch (Configuration.FreezeMode) {
            case FreezeMode.Suspend: {
                ProcessManagement.ResumeProcess(rainmeterPid);
                break;
            }
            case FreezeMode.LowPriority: {
                Process.GetProcessById(rainmeterPid).PriorityClass = ProcessPriorityClass.Normal;
                break;
            }
            default: throw new UnreachableException();
        }
    }

    /// <summary>
    /// This method will get called every time the active foreground window changes.
    /// </summary>
    static void HandleWindowChanged(nint hWinEventHook, uint eventType, nint hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
    {
        if (!RefreshRainmeterPid())
            return;

        if(CheckIfShouldFreeze()) {
            Freeze();
        } else {
            Unfreeze();
        }
    }

    /// <summary>
    /// Returns a value indicating whether the currently stored Rainmeter
    /// process ID is valid.
    /// </summary>
    static bool ValidateRainmeterPid()
    {
        if (rainmeterPid == -1)
            return false;

        try {
            var process = Process.GetProcessById(rainmeterPid);
            return !process.HasExited && process.ProcessName == "Rainmeter";
        } catch (ArgumentException) {
            return false;
        }
    }

    /// <summary>
    /// Refreshes the stored Rainmeter process ID, if necessary.
    /// </summary>
    /// <returns>'true' if the process ID was successfully fetched, or if a refresh is not required at the given time.</returns>
    static bool RefreshRainmeterPid()
    {
        if (ValidateRainmeterPid())
            return true;

        var processes = Process.GetProcessesByName("Rainmeter");

        if (processes.Length == 0) {
            // We couldn't find any Rainmeter processes...
            rainmeterPid = -1;
            return false;
        }

        // At least one Rainmeter process has been found
        rainmeterPid = processes[0].Id;
        return true;
    }

    /// <summary>
    /// Exits the application.
    /// </summary>
    public static void Exit()
    {
        trayIcon.Icon.Dispose();
        User32.UnhookWinEvent(windowChangeHookPtr);

        if (ValidateRainmeterPid()) {
            ProcessManagement.ResumeProcess(rainmeterPid);
        }

        Environment.Exit(0);
    }

    /// <summary>
    /// The path to the application's data folder.
    /// </summary>
    public readonly static string DataFolderPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RainmeterFreeze"
    );
    
    readonly static string StacktraceLogPath = Path.Combine(DataFolderPath, "stacktrace.log");

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
