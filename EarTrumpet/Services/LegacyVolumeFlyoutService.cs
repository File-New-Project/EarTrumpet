using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace EarTrumpet.Services
{
    public class LegacyVolumeFlyoutService
    {
        static class Interop
        {
            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            public static extern int GetClassName(IntPtr hWnd, StringBuilder strText, int maxCount);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

            [DllImport("user32.dll")]
            public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
            public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

            [DllImport("user32.dll")]
            public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

            public const int WM_USERMAGIC = 1120;
            public const int SNDVOL_ACTION_SHOWCONTEXTMENU = 123;

            public static uint MAKEWPARAM(ushort low, ushort high)
            {
                return ((uint)high << 16) | (uint)low;
            }
        }

        public static void ShowMenu()
        {
            var handle = FindATLWindow();
            if (handle != IntPtr.Zero)
            {
                var pt = System.Windows.Forms.Cursor.Position;
                Interop.SendMessage(handle, Interop.WM_USERMAGIC, new IntPtr(Interop.MAKEWPARAM((ushort)pt.X, (ushort)pt.Y)), new IntPtr(Interop.SNDVOL_ACTION_SHOWCONTEXTMENU));
            }
        }

        private static IntPtr FindATLWindow()
        {
            // Find a window in explorer.exe which has a class name starting with ATL:
            IntPtr ret = IntPtr.Zero;
            Interop.EnumWindows(delegate (IntPtr hWnd, IntPtr param)
            {
                StringBuilder clsName = new StringBuilder(128);
                Interop.GetClassName(hWnd, clsName, clsName.Capacity);

                if (clsName.ToString().StartsWith("ATL:"))
                {
                    Interop.GetWindowThreadProcessId(hWnd, out uint processId);
                    try
                    {
                        var proc = Process.GetProcessById((int)processId);

                        if (proc.ProcessName.ToLower() == "explorer")
                        {
                            ret = hWnd;
                        }
                    }
                    catch(Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }

                return ret == IntPtr.Zero;
            }, IntPtr.Zero);
            return ret;
        }
    }
}
