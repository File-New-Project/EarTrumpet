using EarTrumpet.Interop;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace EarTrumpet.Extensions
{
    public static class WindowExtensions
    {
        public static void Move(this Window window, double top, double left, double height, double width)
        {
            User32.SetWindowPos(new WindowInteropHelper(window).Handle, IntPtr.Zero, (int)left, (int)top, (int)width, (int)height, User32.WindowPosFlags.SWP_NOZORDER | User32.WindowPosFlags.SWP_NOACTIVATE);
        }

        public static void RaiseWindow(this Window window)
        {
            window.Topmost = true;
            window.Activate();
            window.Topmost = false;
        }

        public static void Cloak(this Window window, bool hide = true)
        {
            int attributeValue = hide ? 1 : 0;
            DwmApi.DwmSetWindowAttribute(new WindowInteropHelper(window).Handle, DwmApi.DWMA_CLOAK, ref attributeValue, Marshal.SizeOf(attributeValue));
        }

        public static void ApplyExtendedWindowStyle(this Window window, int newExStyle)
        {
            var interop = new WindowInteropHelper(window);
            var currentExStyle = User32.GetWindowLong(interop.Handle, User32.GWL.GWL_EXSTYLE);
            if (currentExStyle == 0)
            {
                Trace.WriteLine($"Failed to apply window styles ({Marshal.GetLastWin32Error()})");
                return;
            }

            var oldExStyle = User32.SetWindowLong(interop.Handle, User32.GWL.GWL_EXSTYLE, currentExStyle | newExStyle);
            if (oldExStyle != currentExStyle)
            {
                Trace.WriteLine($"Unexpected return from SetWindowLong ({oldExStyle} vs. {currentExStyle})");
                return;
            }
        }
    }
}
