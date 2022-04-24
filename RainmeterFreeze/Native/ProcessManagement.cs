using System;
using System.Diagnostics;

namespace RainmeterFreeze.Native {
    /// <summary>
    /// Provides additional methods in order to help with process management.
    /// </summary>
    internal static class ProcessManagement {
        /// <summary>
        /// Suspends a process by its ID.
        /// </summary>
        /// <param name="pid">The target process's ID (PID).</param>
        internal static void SuspendProcess(int pid)
        {
            var process = Process.GetProcessById(pid); // throws exception if process does not exist

            foreach (ProcessThread pT in process.Threads) {
                IntPtr pOpenThread = Kernel32.OpenThread(
                    Kernel32.ThreadAccess.SUSPEND_RESUME,
                    false,
                    (uint)pT.Id
                );

                if (pOpenThread == IntPtr.Zero) {
                    continue;
                }

                Kernel32.SuspendThread(pOpenThread);
                Kernel32.CloseHandle(pOpenThread);
            }
        }

        /// <summary>
        /// Resumes a previously suspended process by its ID.
        /// </summary>
        /// <param name="pid">The target process's ID (PID).</param>
        internal static void ResumeProcess(int pid)
        {
            var process = Process.GetProcessById(pid);

            if (process.ProcessName == string.Empty)
                return;

            foreach (ProcessThread pT in process.Threads) {
                IntPtr pOpenThread = Kernel32.OpenThread(
                    Kernel32.ThreadAccess.SUSPEND_RESUME,
                    false,
                    (uint)pT.Id
                );

                if (pOpenThread == IntPtr.Zero) {
                    continue;
                }

                var suspendCount = 0;
                do {
                    suspendCount = Kernel32.ResumeThread(pOpenThread);
                } while (suspendCount > 0);

                Kernel32.CloseHandle(pOpenThread);
            }
        }
    }
}
