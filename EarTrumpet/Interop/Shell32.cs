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

        [DllImport("shell32.dll", PreserveSig = true)]
        public static extern IntPtr SHAppBarMessage(
            uint dwMessage,
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
