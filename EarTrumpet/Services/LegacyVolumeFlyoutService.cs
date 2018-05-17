using EarTrumpet.Interop;
using System;
using System.Diagnostics;
using System.Text;

namespace EarTrumpet.Services
{
    public class LegacyVolumeFlyoutService
    {
        public static void ShowMenu()
        {
            var handle = FindATLWindow();
            if (handle != IntPtr.Zero)
            {
                var pt = System.Windows.Forms.Cursor.Position;
                User32.SendMessage(handle, User32.WM_USERMAGIC, new IntPtr(User32.MAKEWPARAM((ushort)pt.X, (ushort)pt.Y)), new IntPtr(User32.SNDVOL_ACTION_SHOWCONTEXTMENU));
            }
        }

        private static IntPtr FindATLWindow()
        {
            // Find a window in explorer.exe which has a class name starting with ATL:
            IntPtr ret = IntPtr.Zero;
            User32.EnumWindows(delegate (IntPtr hWnd, IntPtr param)
            {
                StringBuilder clsName = new StringBuilder(128);
                User32.GetClassName(hWnd, clsName, clsName.Capacity);

                if (clsName.ToString().StartsWith("ATL:"))
                {
                    User32.GetWindowThreadProcessId(hWnd, out uint processId);
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
