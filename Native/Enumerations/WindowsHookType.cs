namespace RainmeterFreeze.Native.Enumerations;

enum WindowsHookType
{
    MsgFilter = -1,
    [Obsolete] JournalRecord = 0,
    [Obsolete] JournalPlayback = 1,
    Keyboard = 2,
    GetMessage = 3,
    CallWndProc = 4,
    Cbt = 5,
    SysMsgFilter = 6,
    Mouse = 7,
    Debug = 9,
    Shell = 10,
    ForegroundIdle = 11,
    CallWndProcRet = 12,
    KeyboardLowLevel = 13,
    MouseLowLevel = 14
}