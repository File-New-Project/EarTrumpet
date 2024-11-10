using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using EarTrumpet.Interop;
using Windows.Win32;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.UI.WindowsAndMessaging;

namespace EarTrumpet.Extensions;

public static class WindowExtensions
{
    public static void SetWindowPos(this Window window, double top, double left, double height, double width)
    {
        PInvoke.SetWindowPos(new HWND(window.GetHandle()), HWND.Null, (int)left, (int)top, (int)width, (int)height, SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE);
    }

    public static void RaiseWindow(this Window window)
    {
        window.Topmost = true;
        window.Activate();
        window.Topmost = false;
    }

    public static void Cloak(this Window window, bool hide = true)
    {
        var attributeValue = hide ? 1 : 0;
        unsafe
        {
            _ = PInvoke.DwmSetWindowAttribute(new HWND(window.GetHandle()), DWMWINDOWATTRIBUTE.DWMWA_CLOAK, &attributeValue, (uint)Marshal.SizeOf(attributeValue));
        }
    }

    public static void EnableRoundedCornersIfApplicable(this Window window)
    {
        if (Environment.OSVersion.IsAtLeast(OSVersions.Windows11))
        {
            var attributeValue = (int)DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
            unsafe
            {
                _ = PInvoke.DwmSetWindowAttribute(new HWND(window.GetHandle()), DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, &attributeValue, (uint)Marshal.SizeOf(attributeValue));
            }
        }
    }

    public static void RemoveWindowStyle(this Window window, WINDOW_STYLE styleToRemove)
    {
        var currentStyle = User32.GetWindowLong(new HWND(window.GetHandle()), WINDOW_LONG_PTR_INDEX.GWL_STYLE);
        if (currentStyle == 0)
        {
            Trace.WriteLine($"WindowExtensions RemoveWindowStyle Failed: ({Marshal.GetLastWin32Error()})");
            return;
        }

        _ = User32.SetWindowLong(new HWND(window.GetHandle()), WINDOW_LONG_PTR_INDEX.GWL_STYLE, (int)currentStyle & ~(int)styleToRemove);
    }

    public static void ApplyExtendedWindowStyle(this Window window, WINDOW_EX_STYLE newExStyle)
    {
        var currentExStyle = User32.GetWindowLong(new HWND(window.GetHandle()), WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
        if (currentExStyle == 0)
        {
            Trace.WriteLine($"WindowExtensions ApplyExtendedWindowStyle Failed: ({Marshal.GetLastWin32Error()})");
            return;
        }

        var oldExStyle = User32.SetWindowLong(new HWND(window.GetHandle()), WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, (int)currentExStyle | (int)newExStyle);
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
