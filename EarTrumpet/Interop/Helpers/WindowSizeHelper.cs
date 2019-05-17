using System;
using System.Windows;
using System.Windows.Interop;

namespace EarTrumpet.Interop.Helpers
{
    class WindowSizeHelper
    {
        public static void RestrictMaximizedSizeToWorkArea(Window window)
        {
            var hwnd = ((HwndSource)PresentationSource.FromVisual(window)).Handle;
            var workArea = System.Windows.Forms.Screen.FromHandle(hwnd).WorkingArea;
            User32.SetWindowPos(
                hwnd,
                IntPtr.Zero,
                workArea.Left,
                workArea.Top,
                workArea.Width,
                workArea.Height,
                User32.WindowPosFlags.SWP_NOZORDER | User32.WindowPosFlags.SWP_NOACTIVATE);
        }
    }
}
