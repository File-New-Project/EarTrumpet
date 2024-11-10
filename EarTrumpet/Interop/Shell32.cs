using Windows.Win32;

namespace Windows.Win32
{
    public static partial class PInvoke
    {
        // Missing constants https://github.com/microsoft/win32metadata/issues/1765
        public const int NIN_KEYSELECT = (int)NIN_SELECT | (int)NINF_KEY;
    }
}

namespace EarTrumpet.Interop
{
    class Shell32
    {
        public static readonly int WM_TASKBARCREATED = (int)PInvoke.RegisterWindowMessage("TaskbarCreated");
    }
}
