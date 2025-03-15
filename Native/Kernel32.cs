using System;
using System.Runtime.InteropServices;

namespace RainmeterFreeze.Native;

/// <summary>
/// Provides native methods from the KERNEL32 Dynamic Link Library.
/// </summary>
static partial class Kernel32
{
    [Flags]
    internal enum ThreadAccess : int
    {
        /// <summary>
        /// Required to terminate a thread using TerminateThread.
        /// </summary>
        Terminate = (0x0001),

        /// <summary>
        /// Required to suspend or resume a thread (see SuspendThread and ResumeThread).
        /// </summary>
        SuspendResume = (0x0002),

        /// <summary>
        /// Required to read the context of a thread using GetThreadContext.
        /// </summary>
        GetContext = (0x0008),

        /// <summary>
        /// Required to write the context of a thread using SetThreadContext.
        /// </summary>
        SetContext = (0x0010),

        /// <summary>
        /// Required to set certain information in the thread object.
        /// </summary>
        SetInformation = (0x0020),

        /// <summary>
        /// Required to read certain information from the thread object, such as the exit code (see GetExitCodeThread).
        /// </summary>
        QueryInformation = (0x0040),

        /// <summary>
        /// Required to set the impersonation token for a thread using SetThreadToken.
        /// </summary>
        SetThreadToken = (0x0080),

        /// <summary>
        /// Required to use a thread's security information directly without calling it by using a communication mechanism that provides impersonation services.
        /// </summary>
        Impersonate = (0x0100),

        /// <summary>
        /// Required for a server thread that impersonates a client.
        /// </summary>
        DirectImpersonation = (0x0200)
    }

    /// <summary>
    /// Opens an existing thread object.
    /// </summary>
    /// <param name="dwDesiredAccess">The access to the thread object. This access right is checked against the security descriptor for the thread. This parameter can be one or more of the thread access rights.</param>
    /// <param name="bInheritHandle">If this value is TRUE, processes created by this process will inherit the handle. Otherwise, the processes do not inherit this handle.</param>
    /// <param name="dwThreadId">The identifier of the thread to be opened.</param>
    /// <returns>If the function succeeds, the return value is an open handle to the specified thread. If the function fails, the return value is NULL.</returns>
    [LibraryImport("kernel32.dll")]
    internal static partial IntPtr OpenThread(
        ThreadAccess dwDesiredAccess,
        [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
        uint dwThreadId
    );

    /// <summary>
    /// Suspends the specified thread. A 64-bit application can suspend a
    /// WOW64 thread using the Wow64SuspendThread function.
    /// </summary>
    /// <param name="hThread">A handle to the thread that is to be suspended. The handle must have the <see cref="ThreadAccess.SuspendResume"/> access right.</param>
    /// <returns></returns>
    [LibraryImport("kernel32.dll")]
    internal static partial uint SuspendThread(nint hThread);

    /// <summary>
    /// Decrements a thread's suspend count. When the suspend count
    /// is decremented to zero, the execution of the thread is resumed.
    /// </summary>
    /// <param name="hThread">A handle to the thread to be restarted. The handle must have the <see cref="ThreadAccess.SuspendResume"/> access right.</param>
    /// <returns>If the function succeeds, the return value is the thread's previous suspend count. If the function fails, the return value is (DWORD) -1. To get extended error information, call GetLastError.</returns>
    [LibraryImport("kernel32.dll")]
    internal static partial int ResumeThread(nint hThread);

    /// <summary>
    /// Closes an open object handle.
    /// </summary>
    /// <param name="handle">A valid handle to an open object.</param>
    /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. To get extended error information, call GetLastError.</returns>
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool CloseHandle(nint handle);
}
