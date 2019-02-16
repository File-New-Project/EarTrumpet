using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Actions.Interop
{
    class User32
    {
        public static readonly string SHELLHOOK = "SHELLHOOK";
        public const int HSHELL_WINDOWCREATED = 1;
        public const int HSHELL_WINDOWDESTROYED = 2;

        [DllImport("user32.dll", PreserveSig = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RegisterShellHookWindow(IntPtr hWnd);

        [DllImport("user32.dll", PreserveSig = true)]
        public static extern uint RegisterWindowMessageW([MarshalAs(UnmanagedType.LPWStr)] string msg);
    }
}
