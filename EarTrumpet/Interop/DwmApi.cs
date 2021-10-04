using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop
{
    class DwmApi
    {
        internal const int DWMA_CLOAK = 13;
        internal const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;

        internal enum DWM_WINDOW_CORNER_PREFERENCE
        {
            DWMWCP_DEFAULT = 0,
            DWMWCP_DONOTROUND = 1,
            DWMWCP_ROUND = 2,
            DWMWCP_ROUNDSMALL = 3
        }

        [DllImport("dwmapi.dll", PreserveSig = false)]
        internal static extern void DwmSetWindowAttribute(
            IntPtr hwnd,
            int attr,
            ref int attrValue,
            int attrSize);
    }
}
