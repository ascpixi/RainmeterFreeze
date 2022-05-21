using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using RainmeterFreeze.Native;
using RainmeterFreeze.Enumerations;

namespace RainmeterFreeze {
    /// <summary>
    /// Represents RainmeterFreeze's main <see cref="ApplicationContext"/>.
    /// </summary>
    public sealed class RainmeterFreezeAppContext : ApplicationContext {
        /// <summary>
        /// Invoked when a property of a <see cref="RainmeterFreezeAppContext"/>
        /// is changed.
        /// </summary>
        /// <param name="ctx">The app context in question.</param>
        public delegate void ConfigurationChangedHandler(
            RainmeterFreezeAppContext ctx
        );

        /// <summary>
        /// The configuration currently used by the application.
        /// </summary>
        public readonly AppConfiguration configuration;

        readonly User32.WinEventDelegate windowChangeHandler;
        readonly IntPtr windowChangeHookPtr;
        readonly TrayIcon trayIcon;

        /// <summary>
        /// Occurs when the freeze algorithm of this <see cref="RainmeterFreezeAppContext"/> has changed.
        /// </summary>
        public event ConfigurationChangedHandler FreezeAlgorithmChanged;

        /// <summary>
        /// Occurs when the freeze mode of this <see cref="RainmeterFreezeAppContext"/> has changed.
        /// </summary>
        public event ConfigurationChangedHandler FreezeModeChanged;

        int rainmeterPid = -1; // the current process ID of Rainmeter

        internal RainmeterFreezeAppContext()
        {
            PerformAlreadyRunningCheck();

            try {
                configuration = AppConfiguration.Load();
            } catch (JsonException ex) {
                DialogResult dr = MessageBox.Show(
                    $"The RainmeterFreeze configuration file is corrupted and cannot be parsed.\n\n{ex.Message}\n\nDo you wish to reset the configuration file and continue, or cancel and exit the application?",
                    "RainmeterFreeze Configuration Error",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Error
                );

                if(dr == DialogResult.Cancel) {
                    Environment.Exit(1);
                    return;
                }

                configuration = new AppConfiguration();
                configuration.Save();
            }

            windowChangeHandler = new User32.WinEventDelegate(HandleWindowChanged);
            windowChangeHookPtr = User32.SetWinEventHook(
                User32.EVENT_SYSTEM_FOREGROUND,
                User32.EVENT_SYSTEM_FOREGROUND,
                IntPtr.Zero,
                windowChangeHandler,
                0, 0,
                User32.WINEVENT_OUTOFCONTEXT
            );

            trayIcon = new TrayIcon(this);
        }

        static void PerformAlreadyRunningCheck()
        {
            Process thisProcess = Process.GetCurrentProcess();

            // Process.MainModule will throw an exception if we try to access a process like System (PID 4)!
            if (Process.GetProcesses().Where(x => x.Id != thisProcess.Id && x.ProcessName == thisProcess.ProcessName && x.MainModule.FileName == thisProcess.MainModule.FileName).Any()) {
                MessageBox.Show(
                    "An existing instance of RainmeterFreeze is already running.",
                    "RainmeterFreeze",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                Environment.Exit(2);
            }
        }

        /// <summary>
        /// Sets the algorithm to use when determining when to freeze Rainmeter.
        /// </summary>
        /// <param name="algorithm">The target algorithm to use.</param>
        public void SetFreezeAlgorithm(FreezeAlgorithm algorithm)
        {
            configuration.FreezeAlgorithm = algorithm;
            FreezeAlgorithmChanged(this);
            configuration.Save();
        }

        /// <summary>
        /// Sets the mode to use when freezing or un-freezing Rainmeter.
        /// </summary>
        /// <param name="mode">The target mode to use.</param>
        public void SetFreezeMode(FreezeMode mode)
        {
            if (RefreshRainmeterPid()) {
                Unfreeze();
            }

            configuration.FreezeMode = mode;
            FreezeModeChanged(this);
            configuration.Save();

            if (CheckIfShouldFreeze()) {
                Freeze();
            }
        }

        bool CheckIfShouldFreeze()
        {
            IntPtr hwnd = User32.GetForegroundWindow();

            if (
                hwnd == User32.FindWindow("Shell_TrayWnd", null) ||
                hwnd == User32.GetDesktopWindow() ||
                hwnd == User32.GetShellWindow()
            )
                return false;

            StringBuilder className = new StringBuilder(32);
            User32.GetClassName(hwnd, className, className.Capacity);

            switch (className.ToString()) {
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

            if(Process.GetProcessById((int)pid).ProcessName == "Rainmeter") {
                return false;
            }

            // We have determined that the active window is not a Rainmeter or
            // the desktop window - if our algorithm is "Not on desktop", then
            // we simply return true
            if(configuration.FreezeAlgorithm == FreezeAlgorithm.NotOnDesktop)
                return true;

            // ...however, with the other algorithms, we have to perform additional
            // window checks.
            if(configuration.FreezeAlgorithm == FreezeAlgorithm.Maximized) {
                // Wait for any maximization animation to end.
                // IsZoomed returns false when the maximize animation is playing,
                // and when not given enough time for Rainmeter to process that
                // its not in the foreground anymore, the widgets can stay on top.
                Thread.Sleep(100);
                return User32.IsZoomed(hwnd);
            }

            if (configuration.FreezeAlgorithm == FreezeAlgorithm.FullScreen)
                return ScreenInfo.IsForegroundFullScreen();

            throw new NotImplementedException($"Unknown algorithm '{configuration.FreezeAlgorithm}'");
        }

        void Freeze()
        {
            switch (configuration.FreezeMode) {
                case FreezeMode.Suspend:
                    ProcessManagement.SuspendProcess(rainmeterPid);
                    break;
                case FreezeMode.LowPriority:
                    Process.GetProcessById(rainmeterPid).PriorityClass = ProcessPriorityClass.BelowNormal;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        void Unfreeze()
        {
            switch (configuration.FreezeMode) {
                case FreezeMode.Suspend:
                    ProcessManagement.ResumeProcess(rainmeterPid);
                    break;
                case FreezeMode.LowPriority:
                    Process.GetProcessById(rainmeterPid).PriorityClass = ProcessPriorityClass.Normal;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// This method will get called every time the active foreground window changes.
        /// </summary>
        private void HandleWindowChanged(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (!RefreshRainmeterPid()) return;

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
        bool ValidateRainmeterPid()
        {
            if (rainmeterPid == -1) return false;

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
        /// <returns>Whether the process ID was successfully fetched, or if a refresh is not required at the given time.</returns>
        bool RefreshRainmeterPid()
        {
            if(!ValidateRainmeterPid()) {
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

            return true;
        }

        /// <summary>
        /// Exits the application.
        /// </summary>
        public void Exit()
        {
            trayIcon.Icon.Visible = false;
            trayIcon.Icon.Dispose();
            User32.UnhookWinEvent(windowChangeHookPtr);

            if (ValidateRainmeterPid()) {
                ProcessManagement.ResumeProcess(rainmeterPid);
            }

            Application.Exit();
        }
    }
}
