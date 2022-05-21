using System.Windows.Forms;
using RainmeterFreeze.Properties;
using RainmeterFreeze.Enumerations;

namespace RainmeterFreeze {
    /// <summary>
    /// Handles the RainmeterFreeze tray icon.
    /// </summary>
    public class TrayIcon {
        readonly NotifyIcon trayIcon;
        readonly ToolStripMenuItem algoNotOnDesktopToggle;
        readonly ToolStripMenuItem algoMaximizedToggle;
        readonly ToolStripMenuItem algoFullscreenToggle;

        readonly ToolStripMenuItem modeSuspendToggle;
        readonly ToolStripMenuItem modeLowPriorityToggle;

        /// <summary>
        /// The actual icon displayed in the tray.
        /// </summary>
        public NotifyIcon Icon => trayIcon;

        /// <summary>
        /// Creates a new tray icon, belonging to the specified application context.
        /// </summary>
        /// <param name="owner">The owner of this tray icon.</param>
        public TrayIcon(RainmeterFreezeAppContext owner)
        {
            algoNotOnDesktopToggle = new ToolStripMenuItem(
                "Not on desktop",
                null,
                (s, e) => owner.SetFreezeAlgorithm(FreezeAlgorithm.NotOnDesktop)
            ) { Checked = owner.configuration.FreezeAlgorithm == FreezeAlgorithm.NotOnDesktop };

            algoMaximizedToggle = new ToolStripMenuItem(
                "Foreground window is maximized",
                null,
                (s, e) => owner.SetFreezeAlgorithm(FreezeAlgorithm.Maximized)
            ) { Checked = owner.configuration.FreezeAlgorithm == FreezeAlgorithm.Maximized };

            algoFullscreenToggle = new ToolStripMenuItem(
                "When in full-screen mode",
                null,
                (s, e) => owner.SetFreezeAlgorithm(FreezeAlgorithm.FullScreen)
            ) { Checked = owner.configuration.FreezeAlgorithm == FreezeAlgorithm.FullScreen };

            modeSuspendToggle = new ToolStripMenuItem(
                "Suspend",
                null,
                (s, e) => owner.SetFreezeMode(FreezeMode.Suspend)
            ) { Checked = owner.configuration.FreezeMode == FreezeMode.Suspend };

            modeLowPriorityToggle = new ToolStripMenuItem(
                "Low Priority",
                null,
                (s, e) => owner.SetFreezeMode(FreezeMode.LowPriority)
            ) { Checked = owner.configuration.FreezeMode == FreezeMode.LowPriority };

            trayIcon = new NotifyIcon() {
                Icon = Resources.Icon,
                ContextMenuStrip = new ContextMenuStrip() {
                    Items = {
                        new ToolStripMenuItem("Freeze when...") {
                            Enabled = false
                        },
                        algoNotOnDesktopToggle,
                        algoMaximizedToggle,
                        algoFullscreenToggle,
                        new ToolStripSeparator(),

                        new ToolStripMenuItem("Mode") {
                            Enabled = false
                        },
                        modeSuspendToggle,
                        modeLowPriorityToggle,
                        new ToolStripSeparator(),

                        new ToolStripMenuItem("Exit", null, (s, e) => owner.Exit())
                    }
                },
                Visible = true
            };

            AttachEvents(owner);
        }
    
        private void AttachEvents(RainmeterFreezeAppContext target)
        {
            target.FreezeModeChanged += OnFreezeModeChanged;
            target.FreezeAlgorithmChanged += OnFreezeAlgorithmChanged;
        }

        private void OnFreezeAlgorithmChanged(RainmeterFreezeAppContext ctx)
        {
            var config = ctx.configuration;
            algoNotOnDesktopToggle.Checked = config.FreezeAlgorithm == FreezeAlgorithm.NotOnDesktop;
            algoMaximizedToggle.Checked = config.FreezeAlgorithm == FreezeAlgorithm.Maximized;
            algoFullscreenToggle.Checked = config.FreezeAlgorithm == FreezeAlgorithm.FullScreen;
        }

        private void OnFreezeModeChanged(RainmeterFreezeAppContext ctx)
        {
            var config = ctx.configuration;
            modeSuspendToggle.Checked = config.FreezeMode == FreezeMode.Suspend;
            modeLowPriorityToggle.Checked = config.FreezeMode == FreezeMode.LowPriority;
        }
    }
}
