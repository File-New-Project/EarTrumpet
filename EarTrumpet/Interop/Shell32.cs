using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop
{
    class Shell32
    {
        public const int KF_FLAG_DONT_VERIFY = 0x00004000;

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        [return: MarshalAs(UnmanagedType.Interface)]
        public static extern IShellItem2 SHCreateItemInKnownFolder(
            ref Guid kfid,
            uint dwKFFlags,
            [MarshalAs(UnmanagedType.LPWStr)]string pszItem,
            ref Guid riid);

        [DllImport("shell32.dll", PreserveSig = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SHAppBarMessage(
            AppBarMessage dwMessage, 
            ref APPBARDATA pData);

        [DllImport("shell32.dll", PreserveSig = true)]
        public static extern IntPtr ExtractIcon(
            IntPtr instanceHandle, 
            string path, 
            int iconIndex);
    }
}
