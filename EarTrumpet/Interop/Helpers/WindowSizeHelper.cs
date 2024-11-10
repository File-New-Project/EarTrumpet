using System.Windows;
using System.Windows.Interop;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace EarTrumpet.Interop.Helpers;

internal class WindowSizeHelper
{
    public static void RestrictMaximizedSizeToWorkArea(Window window)
    {
        var hwnd = ((HwndSource)PresentationSource.FromVisual(window)).Handle;
        var workArea = System.Windows.Forms.Screen.FromHandle(hwnd).WorkingArea;
        unsafe
        {
            PInvoke.SetWindowPos(
            new HWND(hwnd.ToPointer()),
            (HWND)null,
            workArea.Left,
            workArea.Top,
            workArea.Width,
            workArea.Height,
            SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE);
        }
    }
}
