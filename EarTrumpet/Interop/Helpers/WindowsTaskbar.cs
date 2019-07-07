using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace EarTrumpet.Interop.Helpers
{
    public sealed class WindowsTaskbar
    {
        public struct State
        {
            public Position Location;
            public RECT Size;
            public Screen ContainingScreen;
            public bool IsAutoHideEnabled;
        }

        // Must match AppBarEdge enum
        public enum Position
        {
            Left = 0,
            Top = 1,
            Right = 2,
            Bottom = 3
        }

        public static uint Dpi => User32.GetDpiForWindow(GetHwnd());

        public static State Current
        {
            get
            {
                var hWnd = GetHwnd();
                var state = new State
                {
                    ContainingScreen = Screen.FromHandle(hWnd),
                };
                var appBarData = new APPBARDATA
                {
                    cbSize = (uint)Marshal.SizeOf(typeof(APPBARDATA)),
                    hWnd = hWnd
                };

                // SHAppBarMessage: Understands Taskbar auto-hide
                // state (the window is positioned across screens).
                if (Shell32.SHAppBarMessage(AppBarMessage.GetTaskbarPos, ref appBarData) != UIntPtr.Zero)
                {
                    state.Size = appBarData.rect;
                    state.Location = (Position)appBarData.uEdge;
                }
                else
                {
                    User32.GetWindowRect(hWnd, out state.Size);

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

                var appBarState = (Shell32.AppBarState)Shell32.SHAppBarMessage(AppBarMessage.GetState, ref appBarData);
                state.IsAutoHideEnabled = appBarState.HasFlag(Shell32.AppBarState.ABS_AUTOHIDE);

                Trace.WriteLine($"WindowsTaskbar Current: Location={state.Location}, AutoHide={state.IsAutoHideEnabled}, Taskbar={hWnd}, Size={state.Size}, Monitor={state.ContainingScreen}");
                return state;
            }
        }

        private static IntPtr GetHwnd() => User32.FindWindow("Shell_TrayWnd", null);
    }
}