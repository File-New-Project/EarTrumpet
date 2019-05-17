using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop
{
    class Shell32
    {
        public static readonly int WM_TASKBARCREATED = User32.RegisterWindowMessage("TaskbarCreated");

        public const int KF_FLAG_DONT_VERIFY = 0x00004000;

        [Flags]
        public enum AppBarState
        {
            ABS_AUTOHIDE = 1
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        [return: MarshalAs(UnmanagedType.Interface)]
        public static extern IShellItem2 SHCreateItemInKnownFolder(
            ref Guid kfid,
            uint dwKFFlags,
            [MarshalAs(UnmanagedType.LPWStr)]string pszItem,
            ref Guid riid);

        [DllImport("shell32.dll", PreserveSig = true)]
        public static extern UIntPtr SHAppBarMessage(
            AppBarMessage dwMessage, 
            ref APPBARDATA pData);

        [DllImport("shell32.dll", PreserveSig = true)]
        public static extern IntPtr ExtractIcon(
            IntPtr instanceHandle, 
            string path, 
            int iconIndex);

        public enum NotifyIconMessage : int
        {
            NIM_ADD = 0x00000000,
            NIM_MODIFY = 0x00000001,
            NIM_DELETE = 0x00000002,
            NIM_SETFOCUS = 0x00000003,
            NIM_SETVERSION = 0x00000004,
        }

        public enum NotifyIconNotification : int
        {
            NIN_SELECT = 0x400,
            NIN_KEYSELECT = 0x401,
            NIN_BALLOONSHOW = 0x402,
            NIN_BALLOONHIDE = 0x403,
            NIN_BALLOONTIMEOUT = 0x404,
            NIN_BALLOONUSERCLICK = 0x405,
            NIN_POPUPOPEN = 0x406,
            NIN_POPUPCLOSE = 0x407,
        }

        public static readonly int NOTIFYICON_VERSION_4 = 4;

        [DllImport("shell32.dll", PreserveSig = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool Shell_NotifyIconW(
            NotifyIconMessage message, 
            ref NOTIFYICONDATAW pNotifyIconData);

        [DllImport("shell32.dll", PreserveSig = true)]
        public static extern int Shell_NotifyIconGetRect(ref NOTIFYICONIDENTIFIER identifier, out RECT iconLocation);
    }
}
