using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop
{
    public class DwmApi
    {
        public const int DWMA_CLOAK = 0xD;

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern void DwmSetWindowAttribute(
            IntPtr hwnd,
            int attr,
            ref int attrValue,
            int attrSize);
    }
}
