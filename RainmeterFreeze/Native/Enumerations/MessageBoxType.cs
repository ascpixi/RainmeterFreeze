namespace RainmeterFreeze.Native.Enumerations;

[Flags]
enum MessageBoxType : uint
{
    OK = 0x00000000,
    OKCancel = 0x00000001,
    AbortRetryIgnore = 0x00000002,
    YesNoCancel = 0x00000003,
    YesNo = 0x00000004,
    RetryCancel = 0x00000005,
    CancelTryContinue = 0x00000006,
    Help = 0x00004000,
    IconWarning = 0x00000030,
    IconInformation = 0x00000040,
    IconQuestion = 0x00000020,
    IconError = 0x00000010,
    ApplModal = 0x00000000,
    SystemModal = 0x00001000,
    TaskModal = 0x00002000,
    DefaultDesktopOnly = 0x00020000,
    Right = 0x00080000,
    RightToLeftReading = 0x00100000,
    SetForeground = 0x00010000,
    TopMost = 0x00040000,
    ServiceNotification = 0x00200000
}