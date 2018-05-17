using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop
{
    public class Shell32
    {
        public const int KF_FLAG_DONT_VERIFY = 0x00004000;

        [StructLayout(LayoutKind.Sequential)]
        public struct APPBARDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public uint uCallbackMessage;
            public uint uEdge;
            public Win32.RECT rc;
            public int lParam;
        }

        public enum ABMsg : uint
        {
            ABM_NEW = 0,
            ABM_REMOVE,
            ABM_QUERYPOS,
            ABM_SETPOS,
            ABM_GETSTATE,
            ABM_GETTASKBARPOS,
            ABM_ACTIVATE,
            ABM_GETAUTOHIDEBAR,
            ABM_SETAUTOHIDEBAR,
            ABM_WINDOWPOSCHANGED,
            ABM_SETSTATE
        }

        [DllImport("shell32.dll", PreserveSig = true)]
        public static extern IntPtr SHAppBarMessage(
            ABMsg dwMessage,
            ref APPBARDATA pData);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        public static extern void SHCreateItemInKnownFolder(
            ref Guid kfid,
            uint dwKFFlags,
            [MarshalAs(UnmanagedType.LPWStr)]string pszItem,
            ref Guid riid,
            [MarshalAs(UnmanagedType.Interface)]out IShellItem2 ppv);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        public static extern IntPtr ExtractIcon(
            IntPtr instanceHandle,
            [MarshalAs(UnmanagedType.LPWStr)]string path,
            int iconIndex);

    }
}
