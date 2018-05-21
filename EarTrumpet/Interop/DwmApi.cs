using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop
{
    class DwmApi
    {
        internal const int DWMA_CLOAK = 13;

        [DllImport("dwmapi.dll", PreserveSig = false)]
        internal static extern void DwmSetWindowAttribute(
            IntPtr hwnd,
            int attr,
            ref int attrValue,
            int attrSize);
    }
}
