﻿using System.Diagnostics;

namespace RainmeterFreeze.Native;

/// <summary>
/// Provides additional methods in order to help with process management.
/// </summary>
static class ProcessManagement
{
    /// <summary>
    /// Suspends a process by its ID.
    /// </summary>
    /// <param name="pid">The target process's ID (PID).</param>
    internal static void SuspendProcess(int pid)
    {
        var process = Process.GetProcessById(pid); // throws exception if process does not exist

        foreach (ProcessThread pT in process.Threads) {
            nint pOpenThread = Kernel32.OpenThread(
                Kernel32.ThreadAccess.SuspendResume,
                false,
                (uint)pT.Id
            );

            if (pOpenThread == 0)
                continue;

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
            nint pOpenThread = Kernel32.OpenThread(
                Kernel32.ThreadAccess.SuspendResume,
                false,
                (uint)pT.Id
            );

            if (pOpenThread == 0)
                continue;

            int suspendCount;
            do {
                suspendCount = Kernel32.ResumeThread(pOpenThread);
            } while (suspendCount > 0);

            Kernel32.CloseHandle(pOpenThread);
        }
    }
}
