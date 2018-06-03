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
    }
}
