using RainmeterFreeze.Enumerations;
using H.NotifyIcon.Core;

namespace RainmeterFreeze;

/// <summary>
/// Handles the RainmeterFreeze tray icon.
/// </summary>
public class ControlTrayIcon
{
    readonly TrayIconWithContextMenu trayIcon;
    readonly PopupMenuItem algoNotOnDesktopToggle;
    readonly PopupMenuItem algoMaximizedToggle;
    readonly PopupMenuItem algoFullscreenToggle;

    readonly PopupMenuItem modeSuspendToggle;
    readonly PopupMenuItem modeLowPriorityToggle;

    /// <summary>
    /// The actual icon displayed in the tray.
    /// </summary>
    public TrayIconWithContextMenu Icon => trayIcon;

    /// <summary>
    /// Creates a new tray icon, belonging to the specified application context.
    /// </summary>
    /// <param name="owner">The owner of this tray icon.</param>
    public ControlTrayIcon()
    {
        trayIcon = new() {
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Environment.ProcessPath ?? "")?.Handle ?? default,
            Visibility = IconVisibility.Visible,
            ToolTip = "RainmeterFreeze",
            ContextMenu = new() {
                Items = {
                    new PopupSubMenu("Freeze when..."),
                    (
                        algoNotOnDesktopToggle = new PopupMenuItem(
                            "Not on desktop",
                            (_, _) => Program.SetFreezeAlgorithm(FreezeAlgorithm.NotOnDesktop)
                        ) { Checked = Program.Configuration.FreezeAlgorithm == FreezeAlgorithm.NotOnDesktop }
                    ),
                    (
                        algoMaximizedToggle = new PopupMenuItem(
                            "Foreground window is maximized",
                            (_, _) => Program.SetFreezeAlgorithm(FreezeAlgorithm.Maximized)
                        ) { Checked = Program.Configuration.FreezeAlgorithm == FreezeAlgorithm.Maximized }
                    ),
                    (
                        algoFullscreenToggle = new PopupMenuItem(
                            "When in full-screen mode",
                            (_, _) => Program.SetFreezeAlgorithm(FreezeAlgorithm.FullScreen)
                        ) { Checked = Program.Configuration.FreezeAlgorithm == FreezeAlgorithm.FullScreen }
                    ),

                    new PopupMenuSeparator(),

                    new PopupSubMenu("Mode"),
                    (
                        modeSuspendToggle = new PopupMenuItem(
                            "Suspend",
                            (_, _) => Program.SetFreezeMode(FreezeMode.Suspend)
                        ) { Checked = Program.Configuration.FreezeMode == FreezeMode.Suspend }
                    ),
                    (
                        modeLowPriorityToggle = new PopupMenuItem(
                            "Low Priority",
                            (_, _) => Program.SetFreezeMode(FreezeMode.LowPriority)
                        ) { Checked = Program.Configuration.FreezeMode == FreezeMode.LowPriority }
                    ),
                    
                    new PopupMenuSeparator(),

                    new PopupMenuItem("Exit", (_, _) => Program.Exit())
                }
            }
        };

        trayIcon.Create();

        Program.FreezeModeChanged += OnFreezeModeChanged;
        Program.FreezeAlgorithmChanged += OnFreezeAlgorithmChanged;
    }

    void OnFreezeAlgorithmChanged()
    {
        var cfg = Program.Configuration;
        algoNotOnDesktopToggle.Checked = cfg.FreezeAlgorithm == FreezeAlgorithm.NotOnDesktop;
        algoMaximizedToggle.Checked = cfg.FreezeAlgorithm == FreezeAlgorithm.Maximized;
        algoFullscreenToggle.Checked = cfg.FreezeAlgorithm == FreezeAlgorithm.FullScreen;
    }

    void OnFreezeModeChanged()
    {
        var cfg = Program.Configuration;
        modeSuspendToggle.Checked = cfg.FreezeMode == FreezeMode.Suspend;
        modeLowPriorityToggle.Checked = cfg.FreezeMode == FreezeMode.LowPriority;
    }
}
