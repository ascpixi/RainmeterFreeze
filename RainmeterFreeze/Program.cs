using System;
using System.IO;
using System.Windows.Forms;

namespace RainmeterFreeze {
    static class Program {
        /// <summary>
        /// The path to the application's data folder.
        /// </summary>
        public readonly static string DataFolderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RainmeterFreeze"
        );

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += GlobalExceptionHandler;

            Directory.CreateDirectory(DataFolderPath);

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new RainmeterFreezeAppContext());
        }

        private readonly static string StacktraceLogPath = Path.Combine(
            DataFolderPath,
            "stacktrace.log"
        );

        private static void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            if (!e.IsTerminating) return;

            if(e.ExceptionObject is Exception ex) {
                try {
                    File.WriteAllText(StacktraceLogPath, $"A fatal exception has been thrown and the application cannot continue.\n\n{ex.GetType()}: {ex.Message}\n{ex.StackTrace}");
                }
                catch (Exception dumpErr) {
                    MessageBox.Show($"A fatal exception occured which could not be dumped to a file.\n{ex.GetType()}: {ex.Message}\n{ex.StackTrace}\n\nThe following exception occurred while attempting to dump the information to a file:\n{dumpErr.GetType()}: {dumpErr.Message}\n{dumpErr.StackTrace}", "RainmeterFreeze", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            } else {
                try {
                    File.WriteAllText(StacktraceLogPath, $"An unknown fatal exception has been thrown and the application cannot continue.\n\nException type: {e.ExceptionObject.GetType()}\nException: {e.ExceptionObject}");
                } catch (Exception dumpErr) {
                    MessageBox.Show($"A fatal exception occured which could not be dumped to a file.\n\n{e.ExceptionObject.GetType()}: {e.ExceptionObject}\n\nThe following exception occurred while attempting to dump the information to a file:\n{dumpErr.GetType()}: {dumpErr.Message}\n{dumpErr.StackTrace}", "RainmeterFreeze", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
