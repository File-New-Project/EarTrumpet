﻿using System.Windows;
using System.Windows.Interop;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace EarTrumpet.Interop.Helpers;

class WindowSizeHelper
{
    public static void RestrictMaximizedSizeToWorkArea(Window window)
    {
        var hwnd = ((HwndSource)PresentationSource.FromVisual(window)).Handle;
        var workArea = System.Windows.Forms.Screen.FromHandle(hwnd).WorkingArea;
        PInvoke.SetWindowPos(
            new HWND(hwnd),
            HWND.Null,
            workArea.Left,
            workArea.Top,
            workArea.Width,
            workArea.Height,
            SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE);
    }
}
