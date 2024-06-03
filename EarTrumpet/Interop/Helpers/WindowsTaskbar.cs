using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;

namespace EarTrumpet.Interop.Helpers;

public sealed class WindowsTaskbar
{
    public struct State
    {
        public Position Location;
        public System.Drawing.Rectangle Size;
        public Screen ContainingScreen;
        public bool IsAutoHideEnabled;

        public readonly bool IsHorizontal => Location == Position.Bottom || Location == Position.Top;
    }

    // Must match AppBarEdge enum
    public enum Position
    {
        Left = 0,
        Top = 1,
        Right = 2,
        Bottom = 3
    }

    public static uint Dpi => PInvoke.GetDpiForWindow(new HWND(GetHwnd()));

    public static State Current
    {
        get
        {
            var hwnd = GetHwnd();
            var state = new State
            {
                ContainingScreen = Screen.FromHandle(hwnd),
            };
            var appBarData = new APPBARDATA
            {
                cbSize = (uint)Marshal.SizeOf(typeof(APPBARDATA)),
                hWnd = new HWND(hwnd)
            };

            // SHAppBarMessage: Understands Taskbar auto-hide
            // state (the window is positioned across screens).
            if (PInvoke.SHAppBarMessage(PInvoke.ABM_GETTASKBARPOS, ref appBarData) != UIntPtr.Zero)
            {
                state.Size = appBarData.rc;
                state.Location = (Position)appBarData.uEdge;
            }
            else
            {
                PInvoke.GetWindowRect(new HWND(hwnd), out var size);
                state.Size = (System.Drawing.Rectangle)size;

                if (state.ContainingScreen != null)
                {
                    var screen = state.ContainingScreen;
                    if (state.Size.Bottom == screen.Bounds.Bottom && state.Size.Top == screen.Bounds.Top)
                    {
                        state.Location = (state.Size.Left == screen.Bounds.Left) ? Position.Left : Position.Right;
                    }
                    if (state.Size.Right == screen.Bounds.Right && state.Size.Left == screen.Bounds.Left)
                    {
                        state.Location = (state.Size.Top == screen.Bounds.Top) ? Position.Top : Position.Bottom;
                    }
                }
            }

            var appBarState = PInvoke.SHAppBarMessage(PInvoke.ABM_GETSTATE, ref appBarData);
            state.IsAutoHideEnabled = (appBarState & PInvoke.ABS_AUTOHIDE) == PInvoke.ABS_AUTOHIDE;

            Trace.WriteLine($"WindowsTaskbar Current: Location={state.Location}, AutoHide={state.IsAutoHideEnabled}, Taskbar={hwnd}, Size={state.Size}, Monitor={state.ContainingScreen}");
            return state;
        }
    }

    public static HWND GetHwnd() => PInvoke.FindWindow("Shell_TrayWnd", null);

    public static IntPtr GetTrayToolbarWindowHwnd()
    {
        var hwnd = GetHwnd();
        if (hwnd != IntPtr.Zero)
        {
            hwnd = PInvoke.FindWindowEx(hwnd, HWND.Null, "TrayNotifyWnd", null);
            if (hwnd != IntPtr.Zero)
            {
                hwnd = PInvoke.FindWindowEx(hwnd, HWND.Null, "SysPager", null);
                if (hwnd != IntPtr.Zero)
                {
                    hwnd = PInvoke.FindWindowEx(hwnd, HWND.Null, "ToolbarWindow32", null);
                }
            }
        }
        return hwnd;
    }
}