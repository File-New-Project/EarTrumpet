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
        public static void SetWindowPos(this Window window, double top, double left, double height, double width)
        {
            User32.SetWindowPos(window.GetHandle(), IntPtr.Zero, (int)left, (int)top, (int)width, (int)height, User32.WindowPosFlags.SWP_NOZORDER | User32.WindowPosFlags.SWP_NOACTIVATE);
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
            DwmApi.DwmSetWindowAttribute(window.GetHandle(), DwmApi.DWMA_CLOAK, ref attributeValue, Marshal.SizeOf(attributeValue));
        }

        public static void RemoveWindowStyle(this Window window, int styleToRemove)
        {
            var currentStyle = User32.GetWindowLong(window.GetHandle(), User32.GWL.GWL_STYLE);
            if (currentStyle == 0)
            {
                Trace.WriteLine($"WindowExtensions RemoveWindowStyle Failed: ({Marshal.GetLastWin32Error()})");
                return;
            }

            User32.SetWindowLong(window.GetHandle(), User32.GWL.GWL_STYLE, (currentStyle & ~styleToRemove));
        }

        public static void ApplyExtendedWindowStyle(this Window window, int newExStyle)
        {
            var currentExStyle = User32.GetWindowLong(window.GetHandle(), User32.GWL.GWL_EXSTYLE);
            if (currentExStyle == 0)
            {
                Trace.WriteLine($"WindowExtensions ApplyExtendedWindowStyle Failed: ({Marshal.GetLastWin32Error()})");
                return;
            }

            var oldExStyle = User32.SetWindowLong(window.GetHandle(), User32.GWL.GWL_EXSTYLE, currentExStyle | newExStyle);
            if (oldExStyle != currentExStyle)
            {
                Trace.WriteLine($"WindowExtensions ApplyExtendedWindowStyle Unexpected: ({oldExStyle} vs. {currentExStyle})");
                return;
            }
        }

        public static IntPtr GetHandle(this Window window)
        {
            return new WindowInteropHelper(window).Handle;
        }
    }
}
