using System;
using System.Drawing;
using System.Linq;
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

        public static uint Dpi
        {
            get
            {
                var hWnd = User32.FindWindow("Shell_TrayWnd", null);
                return User32.GetDpiForWindow(hWnd);
            }
        }

        public static State Current
        {
            get
            {
                var hWnd = User32.FindWindow("Shell_TrayWnd", null);
                var state = new State();
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
                    state.ContainingScreen = GetScreenFromRect(state.Size);
                }
                else
                {
                    User32.GetWindowRect(hWnd, out state.Size);
                    var screen = GetScreenFromRect(state.Size);
                    state.ContainingScreen = screen;

                    if (screen != null)
                    {
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

                return state;
            }
        }

        private static Screen GetScreenFromRect(RECT rect)
        {
            return Screen.AllScreens.FirstOrDefault(x => x.Bounds.Contains(
                new Rectangle(
                rect.Left,
                rect.Top,
                rect.Right - rect.Left,
                rect.Bottom - rect.Top)
            ));
        }
    }
}