using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace EarTrumpet.Interop
{
    // Based on code from: https://www.quppa.net/blog/2010/12/08/windows-7-style-notification-area-applications-in-wpf-part-2-notify-icon-position/
    public static class NotifyIconInfo
    {
        public static RECT GetNotifyIconLocation(NotifyIcon notifyIcon)
        {
            FieldInfo idFieldInfo = notifyIcon.GetType().GetField("id", BindingFlags.NonPublic | BindingFlags.Instance);
            int iconid = (int)idFieldInfo.GetValue(notifyIcon);

            FieldInfo windowFieldInfo = notifyIcon.GetType().GetField("window", BindingFlags.NonPublic | BindingFlags.Instance);
            NativeWindow nativeWindow = (NativeWindow)windowFieldInfo.GetValue(notifyIcon);
            IntPtr iconhandle = nativeWindow.Handle;

            RECT rect = new RECT();
            NOTIFYICONIDENTIFIER nid = new NOTIFYICONIDENTIFIER()
            {
                hWnd = iconhandle,
                uID = (uint)iconid
            };
            nid.cbSize = (uint)Marshal.SizeOf(nid);

            int result = Shell_NotifyIconGetRect(ref nid, out rect);

            return rect;
        }

        private struct NOTIFYICONIDENTIFIER
        {
            public uint cbSize;
            public IntPtr hWnd;
            public uint uID;
            public Guid guidItem;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("Shell32", SetLastError = true)]
        private static extern Int32 Shell_NotifyIconGetRect([In] ref NOTIFYICONIDENTIFIER identifier, [Out] out RECT iconLocation);


    }
}
