using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace EarTrumpet.Extensions
{
    internal static class Extensions
    {
        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern uint QueryFullProcessImageName([In] IntPtr hProcess, [In] uint dwFlags, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);

        public static string GetMainModuleFileName(this Process process, int buffer = 1024)
        {
            var fileNameBuilder = new StringBuilder(buffer);
            uint bufferLength = (uint)fileNameBuilder.Capacity + 1;
            return QueryFullProcessImageName(process.Handle, 0, fileNameBuilder, ref bufferLength) != 0 ?
                fileNameBuilder.ToString() :
                null;
        }
    }
}
