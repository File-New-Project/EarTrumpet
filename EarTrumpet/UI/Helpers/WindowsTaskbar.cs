using EarTrumpet.Interop;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace EarTrumpet.UI.Helpers
{
    sealed class WindowsTaskbar
    {
        public struct State
        {
            public Position Location;
            public RECT Size;
            public Screen ContainingScreen;
            public double Dpi;
        }

        // Must match AppBarEdge enum
        public enum Position
        {
            Left = 0,
            Top = 1,
            Right = 2,
            Bottom = 3
        }

        public static State Current
        {
            get
            {
                var state = new State();
                var hWnd = User32.FindWindow("Shell_TrayWnd", null);

                if (Error.S_OK == Shcore.GetDpiForMonitor(User32.MonitorFromWindow(hWnd, User32.MONITOR_DEFAULT.MONITOR_DEFAULTTONEAREST), Shcore.DpiType.Effective, out uint dpiX, out uint dpiY))
                {
                    state.Dpi = dpiY / 96f;
                }

                var appBarData = new APPBARDATA
                {
                    cbSize = (uint)Marshal.SizeOf(typeof(APPBARDATA)),
                    hWnd = hWnd
                };

                // SHAppBarMessage: Understands Taskbar auto-hide
                // state (the window is positioned across screens).

                if (Shell32.SHAppBarMessage(AppBarMessage.GetTaskbarPos, ref appBarData))
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